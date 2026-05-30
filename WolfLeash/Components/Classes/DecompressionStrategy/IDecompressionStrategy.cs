namespace WolfLeash.Components.Classes.DecompressionStrategy;

public interface IDecompressionStrategy
{
    string FileExtension { get; }
    List<string> GetSupportedCompressionFormat() => [FileExtension];
    Task<string> DecompressAsync(
        CompatibilityToolTarget target, 
        Stream archiveStream, 
        string fileExtensions,
        string outputPath,
        CancellationToken ct = default);
}