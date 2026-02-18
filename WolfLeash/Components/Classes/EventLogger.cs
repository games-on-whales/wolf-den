using BlazorBootstrap;

namespace WolfLeash.Components.Classes;

public class EventLogger
{
    private readonly ILogger<EventLogger> _logger;
    private readonly ToastService _toastService;
    
    public EventLogger(ILogger<EventLogger> logger, ToastService toastService)
    {
        _logger = logger;
        _toastService = toastService;
    }

    public void LogEvent(ToastMessage msg, bool? autohideOverwrite = null, bool notify = true)
    {
        if (notify)
        {
            if (autohideOverwrite.HasValue)
            {
                msg.AutoHide = autohideOverwrite.Value;
            }
            else
            {
                msg.AutoHide = msg.Type is not (ToastType.Danger or ToastType.Warning);
            }
            _toastService.Notify(msg);
        }

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (msg.Type)
        {
            case ToastType.Danger:
                _logger.LogError("{message}", msg.Message);
                break;
            case ToastType.Warning:
                _logger.LogWarning("{message}", msg.Message);
                break;
            case ToastType.Info:
                _logger.LogInformation("{message}", msg.Message);
                break;
            default:
                break;
        }
    }
}