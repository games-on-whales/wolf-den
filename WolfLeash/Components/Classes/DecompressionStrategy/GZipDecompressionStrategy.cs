namespace WolfLeash.Components.Classes.DecompressionStrategy;

public class GZipDecompressionStrategy : IDecompressionStrategy, IMultiStageDecompressionStrategy
{
    public string FileExtension { get; } = ".gz";
    public Dictionary<string, IDecompressionStrategy> MultistepDecompressionStrategies { get; }

    public GZipDecompressionStrategy(IEnumerable<IDecompressionStrategy> decompressionStrategies)
    {
        MultistepDecompressionStrategies = decompressionStrategies
            .ToDictionary(x => x.FileExtension, x => x);
    }
    
    public List<string> GetSupportedCompressionFormat()
    {
        return MultistepDecompressionStrategies.Values
            // Prevent infinite loop if multiple MultistageDecompressionStrategy exists 
            .Where(si => !si.GetType().IsAssignableTo(typeof(IMultiStageDecompressionStrategy))) 
            .Select(x => x.FileExtension).ToList();
    }

    public IDecompressionStrategy GetNextDecompressionPass(string fileExtension)
    {
        var lastExtension = Path.GetExtension(fileExtension);
        MultistepDecompressionStrategies.TryGetValue(lastExtension, out var decompressionStrategy);
        return decompressionStrategy ?? throw new NotSupportedException("Decompression strategy not supported");
    }
    
    public async Task<string> DecompressAsync(
        CompatibilityToolTarget target, 
        Stream archiveStream, 
        string fileExtensions,
        string outputPath,
        CancellationToken ct = default)
    {
        await using var stream = new System.IO.Compression.GZipStream(archiveStream, System.IO.Compression.CompressionMode.Decompress);
        
        var lastExtension = Path.GetExtension(fileExtensions);
        fileExtensions = fileExtensions[..^lastExtension.Length];
        return await GetNextDecompressionPass(lastExtension).DecompressAsync(target, stream, fileExtensions, outputPath, ct);
    }
}