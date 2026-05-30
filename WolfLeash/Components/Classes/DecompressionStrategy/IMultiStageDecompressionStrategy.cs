namespace WolfLeash.Components.Classes.DecompressionStrategy;

public interface IMultiStageDecompressionStrategy
{
    Dictionary<string, IDecompressionStrategy> MultistepDecompressionStrategies { get; }
    IDecompressionStrategy GetNextDecompressionPass(string fileExtension);
}