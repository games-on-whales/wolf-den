namespace WolfLeash.Components.Classes.DecompressionStrategy;

public class TarUnpackingStrategy : IDecompressionStrategy
{
    public string FileExtension { get; } = ".tar";
    
    public async Task<string> DecompressAsync(
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
        
        await System.Formats.Tar.TarFile.ExtractToDirectoryAsync(archiveStream, tempExtractRoot, true, ct);
        return tempExtractRoot;
    }
}