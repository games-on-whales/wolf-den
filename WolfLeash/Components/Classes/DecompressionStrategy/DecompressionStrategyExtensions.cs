namespace WolfLeash.Components.Classes.DecompressionStrategy;

public static class DecompressionStrategyExtensions
{
    private static IServiceCollection AddDecompressionStrategy<T>(this IServiceCollection serviceCollection, object? key)
        where T : class, IDecompressionStrategy
    {
        if (typeof(T).IsAssignableTo(typeof(IMultiStageDecompressionStrategy)))
        {
            serviceCollection.AddKeyedTransient<IDecompressionStrategy>(key,(provider, innerKey) =>
            {
                var decompressionStrategies = serviceCollection
                    .Where(sd => sd.IsKeyedService && sd.ServiceType == typeof(IDecompressionStrategy))
                    .Select(d => d.ServiceKey)
                    .Where(s =>
                    {
                        if(s?.GetType() != innerKey!.GetType()) return true;
                        return s != innerKey;
                    })
                    .Select(k => provider.GetRequiredKeyedService<IDecompressionStrategy>(k));
    
                return ActivatorUtilities.CreateInstance<T>(provider, decompressionStrategies);
            });
            return serviceCollection;
        }
        
        serviceCollection.AddKeyedTransient<IDecompressionStrategy, T>(key);
        return serviceCollection;
    }
    

    private static IServiceCollection AddKeyedServicesAsTransient(this IServiceCollection serviceCollection)
    {
        var keys = serviceCollection
            .Where(sd => sd.IsKeyedService && sd.ServiceType == typeof(IDecompressionStrategy))
            .Select(d => d.ServiceKey)
            .OfType<string>();

        serviceCollection.AddTransient<IEnumerable<IDecompressionStrategy>>(p => keys
            .Select(p.GetRequiredKeyedService<IDecompressionStrategy>));
        
        return serviceCollection;
    }
    
    public static IServiceCollection AddDecompressionStrategies(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddDecompressionStrategy<TarUnpackingStrategy>("tar")
            .AddDecompressionStrategy<ZipDecompressionStrategy>("zip")
            .AddDecompressionStrategy<GZipDecompressionStrategy>("gz")
            .AddKeyedServicesAsTransient();
    }
}