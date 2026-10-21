using System.Diagnostics;

namespace WolfLeash.Components.Classes;

public partial class MoonlightWebService : IDisposable
{
    private readonly ILogger<MoonlightWebService> _logger;
    private readonly ActiveUserManager _activeUserManager;
    
    private Process? _moonlightProcess;

    public bool IsRunning
    {
        get => field; 
        private set
        {
            field = value;
            if (field)
            {
                Started?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Stopped?.Invoke(this, EventArgs.Empty);
            }
        }
    } = false;
    
    private readonly string _moonlightWebPath = "/app/moonlight-web";
    private readonly string _moonlightWebApp = "web-server";
    private readonly string _moonlightWebArgs = "--bind-address 127.0.0.1:8088 --path-prefix /moonlight";

    public event EventHandler? Started;
    public event EventHandler? Stopped;
    public event EventHandler? SessionStopped;
    
    public MoonlightWebService(ILogger<MoonlightWebService> logger, IHostApplicationLifetime lifetime, IHttpClientFactory httpClientFactory, ActiveUserManager activeUserManager)
    {
        _logger = logger;
        _activeUserManager = activeUserManager;
        lifetime.ApplicationStopping.Register(Stop);
        
        _activeUserManager.OnActivityStateChanged += ActiveUserManagerOnOnActivityStateChanged;
    }

    private void ActiveUserManagerOnOnActivityStateChanged(bool hasUser)
    {
        if(!hasUser) Stop();
    }

    public async Task Start()
    {
        _logger.LogInformation("Starting moonlight web service");
        if (_moonlightProcess is not null) return;
        
        var startInfo = new ProcessStartInfo
        {
            FileName = $"{_moonlightWebPath}/{_moonlightWebApp}",
            WorkingDirectory = _moonlightWebPath,
            //RedirectStandardOutput = true,
            Arguments = _moonlightWebArgs,
            CreateNoWindow = true
        };
        
        _moonlightProcess = Process.Start(startInfo);
        
        // _moonlightProcess?.OutputDataReceived += (_, e) =>
        // {
        //     if (e.Data is not null)
        //     {
        //         LogData(e.Data);
        //     }
        // };
        
        await Task.Delay(500);
        IsRunning = true;
    }
    
    public void Stop()
    {
        _logger.LogInformation("Stopping moonlight web service");
        if (_moonlightProcess is null) return;
        _moonlightProcess?.Kill();
        _moonlightProcess?.Dispose();
        _moonlightProcess = null;
        IsRunning = false;
    }

    public void Dispose()
    {
        _moonlightProcess?.Kill();
        _moonlightProcess?.Dispose();
        _activeUserManager.OnActivityStateChanged -= ActiveUserManagerOnOnActivityStateChanged;
    }

    [LoggerMessage(LogLevel.Information, "{Data}")]
    partial void LogData(string data);
}