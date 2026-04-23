using System.IO.Compression;

namespace WolfLeash.Components.Classes.DecompressionStrategy;

public class ZipDecompressionStrategy : IDecompressionStrategy
{
    public string FileExtension { get; } = ".zip";

    public Task<string> DecompressAsync(
        CompatibilityToolTarget target,
        Stream archiveStream,
        string fileExtensions,
        string outputPath,
        CancellationToken ct = default)
    {
        var tempExtractRoot = Path.Combine(
            target.Path,
            $".wolfden-extract-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempExtractRoot);
        
        ZipFile.ExtractToDirectory(archiveStream, tempExtractRoot);
        return Task.FromResult(tempExtractRoot);
    }
}