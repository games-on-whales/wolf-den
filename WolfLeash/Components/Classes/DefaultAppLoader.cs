using System.Text.RegularExpressions;
using GamesOnWhales;
using Tomlyn;
using Tomlyn.Model;
using WolfLeash.Extensions;

namespace WolfLeash.Components.Classes;

public partial class DefaultAppLoader
{
    private const string ConfigBaseAddress =
        "https://raw.githubusercontent.com/games-on-whales/gow/refs/heads/master/apps/{0}/assets/wolf.config.toml";
    
    private static readonly string[] DefaultApps = 
        ["es-de", "firefox", "heroic-games-launcher", "kodi", "lutris", "pegasus", "prismlauncher", "retroarch", "steam", "xfce"];

    private static string? _defaultRenderDevice;
    private static string DefaultRenderDevice => _defaultRenderDevice ??= GetDefaultRenderDevice();
    
    private readonly HttpClient _httpClient;
    
    public DefaultAppLoader()
    {
        _httpClient = new HttpClient();
    }

    public Task<List<GamesOnWhales.App>> GetApps() => GetApps(CancellationToken.None);
    public async Task<List<GamesOnWhales.App>> GetApps(CancellationToken cancellationToken)
    {
        var apps = new List<GamesOnWhales.App>();
        foreach (var appName in DefaultApps)
        {
            apps.Add(await GetApp(appName, cancellationToken));
        }
        return apps;
    }
    
    public Task<string> GetAppConfigAsync(string appName) => GetAppConfigAsync(appName, CancellationToken.None);
    public async Task<string> GetAppConfigAsync(string appName, CancellationToken cancellationToken)
    {
        var url = string.Format(ConfigBaseAddress, appName);
        var response = await _httpClient.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return body;
    }

    public Task<GamesOnWhales.App> GetApp(string appName) => GetApp(appName, CancellationToken.None);
    public async Task<GamesOnWhales.App> GetApp(string appName, CancellationToken cancellationToken)
    {
        var cfg = await GetAppConfigAsync(appName, cancellationToken);
        var model = ((TomlTableArray)Toml.ToModel(cfg)["apps"])[0];
        var runner = (TomlTable)model["runner"];
        var app = new GamesOnWhales.App()
        {
            Id = Guid.NewGuid().ToBase64(),
            Title = (string)model["title"],
            Icon_png_path = (string)model["icon_png_path"],
            Start_audio_server = true,
            Start_virtual_compositor = true,
            Support_hdr = false,
            H264_gst_pipeline = "",
            Hevc_gst_pipeline = "",
            Av1_gst_pipeline = "",
            Opus_gst_pipeline = "",
            Render_node = DefaultRenderDevice,
            
            Runner = new Wolf__config__AppDocker__tagged()
            {
                Name = (string)runner["name"],
                Image = (string)runner["image"],
                Base_create_json = (string)runner["base_create_json"],
                Devices = ((TomlArray)runner["devices"]).Select(t => (string)t!).ToList(),
                Env = ((TomlArray)runner["env"]).Select(t => (string)t!).ToList(),
                Mounts = ((TomlArray)runner["mounts"]).Select(t => (string)t!).ToList(),
                Ports = ((TomlArray)runner["ports"]).Select(t => (string)t!).ToList(),
                Type = "docker"
            }
        };
        return app;
    }

    private static string GetDefaultRenderDevice()
    {
        if (!Path.Exists("/dev/dri/"))
        {
            return "/dev/dri/renderD128";
        }

        var renderDevices = Directory.GetFiles("/dev/dri/")
            .Where(f => RendererRegex().IsMatch(f))
            .ToList();

        return renderDevices.Count > 0 ? renderDevices[0] : "/dev/dri/renderD128";
    }
    
    [GeneratedRegex(".*?/renderD[0-9]+")]
    private static partial Regex RendererRegex();
}