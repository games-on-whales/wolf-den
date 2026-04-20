using Microsoft.Extensions.Options;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace WolfLeash.Components.Classes;

public record CompatibilityTool(string Name, string Path, long SizeBytes);

public class CompatibilityToolsService
{
    public static readonly string[] SupportedExtensions =
        [".tar.gz", ".tgz", ".tar.xz", ".txz", ".tar.bz2", ".tbz2", ".tar", ".zip"];

    private readonly IOptionsMonitor<CompatibilityToolsOptions> _options;
    private readonly ILogger<CompatibilityToolsService> _logger;

    public CompatibilityToolsService(
        IOptionsMonitor<CompatibilityToolsOptions> options,
        ILogger<CompatibilityToolsService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public IReadOnlyList<CompatibilityToolTarget> Targets => _options.CurrentValue.Targets;

    public bool TargetExists(CompatibilityToolTarget target) => Directory.Exists(target.Path);

    public IReadOnlyList<CompatibilityTool> List(CompatibilityToolTarget target)
    {
        if (!Directory.Exists(target.Path)) return Array.Empty<CompatibilityTool>();

        return new DirectoryInfo(target.Path)
            .EnumerateDirectories()
            .Where(d => !d.Name.StartsWith(".wolfden-", StringComparison.Ordinal))
            .Select(d => new CompatibilityTool(d.Name, d.FullName, DirectorySize(d)))
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public void Delete(CompatibilityToolTarget target, string toolName)
    {
        var full = ResolveSafe(target, toolName)
            ?? throw new InvalidOperationException($"Invalid tool name '{toolName}'.");

        if (!Directory.Exists(full))
            throw new DirectoryNotFoundException(full);

        Directory.Delete(full, recursive: true);
        _logger.LogInformation("Deleted compatibility tool {Path}", full);
    }

    public async Task<string> ExtractArchiveAsync(
        CompatibilityToolTarget target,
        Stream archiveStream,
        string originalFileName,
        CancellationToken ct = default)
    {
        if (!IsSupported(originalFileName))
            throw new NotSupportedException(
                $"Unsupported archive type for '{originalFileName}'. " +
                $"Supported: {string.Join(", ", SupportedExtensions)}");

        Directory.CreateDirectory(target.Path);

        var tempExtractRoot = Path.Combine(
            target.Path,
            $".wolfden-extract-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempExtractRoot);

        try
        {
            await Task.Run(() =>
            {
                using var reader = ReaderFactory.Open(archiveStream);
                var extractOptions = new ExtractionOptions
                {
                    ExtractFullPath = true,
                    Overwrite = true,
                    PreserveFileTime = true,
                };

                while (reader.MoveToNextEntry())
                {
                    ct.ThrowIfCancellationRequested();
                    if (reader.Entry.IsDirectory) continue;

                    var entryKey = reader.Entry.Key ?? "";
                    if (IsUnsafeEntry(entryKey))
                    {
                        _logger.LogWarning("Skipping unsafe archive entry {Key}", entryKey);
                        continue;
                    }

                    reader.WriteEntryToDirectory(tempExtractRoot, extractOptions);
                }
            }, ct);

            var entries = new DirectoryInfo(tempExtractRoot).GetFileSystemInfos();
            string finalDir;

            if (entries.Length == 1 && entries[0] is DirectoryInfo single)
            {
                finalDir = Path.Combine(target.Path, SanitizeName(single.Name));
                if (Directory.Exists(finalDir))
                    Directory.Delete(finalDir, recursive: true);
                Directory.Move(single.FullName, finalDir);
                Directory.Delete(tempExtractRoot, recursive: true);
            }
            else
            {
                var stem = SanitizeName(StripArchiveSuffix(originalFileName));
                if (string.IsNullOrWhiteSpace(stem)) stem = $"tool-{DateTime.UtcNow:yyyyMMddHHmmss}";
                finalDir = Path.Combine(target.Path, stem);
                if (Directory.Exists(finalDir))
                    Directory.Delete(finalDir, recursive: true);
                Directory.Move(tempExtractRoot, finalDir);
            }

            _logger.LogInformation("Extracted compatibility tool {Archive} -> {Path}", originalFileName, finalDir);
            return finalDir;
        }
        catch
        {
            try { if (Directory.Exists(tempExtractRoot)) Directory.Delete(tempExtractRoot, recursive: true); }
            catch { /* best effort cleanup */ }
            throw;
        }
    }

    public void ProvisionTargets()
    {
        foreach (var target in Targets)
        {
            if (string.IsNullOrWhiteSpace(target.Path)) continue;
            try
            {
                // Only auto-create when the grandparent (the host-mounted root, e.g.
                // /etc/wolf) exists. Otherwise the mount isn't present and creating
                // the tree would just produce empty container-local dirs that
                // wouldn't be visible to the Wolf runner containers.
                var parent = Directory.GetParent(target.Path)?.FullName;
                var grandparent = parent is null ? null : Directory.GetParent(parent)?.FullName;
                if (grandparent is null || !Directory.Exists(grandparent))
                {
                    _logger.LogInformation(
                        "Skipping auto-provision of {Path}: mount root not present.",
                        target.Path);
                    continue;
                }

                Directory.CreateDirectory(target.Path);
                _logger.LogInformation("Ensured compatibility tools directory {Path}", target.Path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to provision {Path}", target.Path);
            }
        }
    }

    public static bool IsSupported(string fileName)
    {
        var lower = fileName.ToLowerInvariant();
        return SupportedExtensions.Any(ext => lower.EndsWith(ext));
    }

    private static bool IsUnsafeEntry(string key)
    {
        if (string.IsNullOrEmpty(key)) return true;
        var normalized = key.Replace('\\', '/');
        if (normalized.StartsWith('/')) return true;
        foreach (var segment in normalized.Split('/'))
        {
            if (segment == "..") return true;
        }
        return false;
    }

    private static string StripArchiveSuffix(string fileName)
    {
        var n = fileName;
        foreach (var suffix in SupportedExtensions)
        {
            if (n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return n[..^suffix.Length];
        }
        return Path.GetFileNameWithoutExtension(n);
    }

    private static string SanitizeName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        if (cleaned is "" or "." or "..") return "tool";
        return cleaned;
    }

    private static string? ResolveSafe(CompatibilityToolTarget target, string toolName)
    {
        if (string.IsNullOrWhiteSpace(toolName)) return null;
        if (toolName.Contains('/') || toolName.Contains('\\') || toolName is "." or "..") return null;

        var rootFull = Path.GetFullPath(target.Path);
        var candidate = Path.GetFullPath(Path.Combine(rootFull, toolName));
        var sep = Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(rootFull + sep, StringComparison.Ordinal)) return null;
        return candidate;
    }

    private static long DirectorySize(DirectoryInfo dir)
    {
        try
        {
            return dir.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
        }
        catch
        {
            return 0;
        }
    }
}
