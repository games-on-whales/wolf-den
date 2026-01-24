using GamesOnWhales;

namespace WolfLeash.Components.Classes;

public class Api : WolfApi
{
    private readonly ILogger<WolfApi> _logger;
    private readonly IHostApplicationLifetime _lifeCycleService;
    private int _connectionErrorCount = 0;
    private DateTime _lastConnectionErrorTime = DateTime.MinValue;
    
    public Api(ILogger<WolfApi> logger, IConfiguration configuration, IHostApplicationLifetime lifetime) : 
        base(logger, configuration)
    {
        _lifeCycleService = lifetime;
        _logger = logger;
    }

    private static Task Raise<TSource, TEventArgs>(Func<TSource, TEventArgs, Task>? handlers, TSource source, TEventArgs args)
    {
        if (handlers != null)
        {
            return Task.WhenAll(handlers.GetInvocationList()
                .OfType<Func<TSource, TEventArgs, Task>>()
                .Select(h => h(source, args)));
        }

        return Task.CompletedTask;
    }

    protected override Task OnSseConnectionLostEvent(bool isFatal)
    {
        if (DateTime.Now - _lastConnectionErrorTime > TimeSpan.FromSeconds(10))
        {
            _connectionErrorCount = 0;
            _lastConnectionErrorTime = DateTime.Now;
        }
        if (_connectionErrorCount++ <= 3) return base.OnSseConnectionLostEvent(isFatal);
        _logger.LogError("Can't connect to Socket, shutting down.");
        _lifeCycleService.StopApplication();

        return base.OnSseConnectionLostEvent(isFatal);
    }

    protected override async Task OnPairSignalEvent(string data)
    {
        await Raise(PairRequestEvent, this, data);
    }

    protected override async Task OnProfilesUpdatedEvent(ICollection<Profile> profiles)
    {
        await Raise(ProfilesUpdatedEvent, this, profiles);
    }

    public event Func<object, ICollection<Profile>, Task>? ProfilesUpdatedEvent;
    public event Func<object, string, Task>? PairRequestEvent;
}