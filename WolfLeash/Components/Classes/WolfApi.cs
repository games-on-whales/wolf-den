using System.Text;
using GamesOnWhales;
using GamesOnWhales.SSE;

namespace WolfLeash.Components.Classes;

public class Api : WolfApi
{
    private readonly ILogger<WolfApi> _logger;
    private readonly IHostApplicationLifetime _lifeCycleService;
    private int _connectionErrorCount = 0;
    private DateTime _lastConnectionErrorTime = DateTime.MinValue;
    
    public Api(ILogger<WolfApi> logger, IConfiguration configuration, IEnumerable<ISseEventHandler> handlers, IHostApplicationLifetime lifetime) : 
        base(logger, configuration, handlers)
    {
        _lifeCycleService = lifetime;
        _logger = logger;
        
        var builder = new StringBuilder();
        foreach (var handler in handlers)
        {
            builder.Append(' ');
            builder.AppendLine(handler.EventName);
        }
        builder.Length -= builder.Length > 0 ? 1 : 0;
        
        logger.LogInformation("Listening for: \n{event}", builder.ToString());
    }

    protected override Task OnSseConnectionLostEvent(bool isFatal)
    {
        if (DateTime.Now - _lastConnectionErrorTime > TimeSpan.FromSeconds(30))
        {
            _connectionErrorCount = 0;
            _lastConnectionErrorTime = DateTime.Now;
        }
        if (++_connectionErrorCount <= 3) return base.OnSseConnectionLostEvent(isFatal);
        _logger.LogError("Can't connect to Socket, shutting down.");
        _lifeCycleService.StopApplication();

        return base.OnSseConnectionLostEvent(isFatal);
    }
}