using Microsoft.AspNetCore.Components.Server.Circuits;

namespace WolfLeash.Components.Classes;

public class BlazorConnectionTracker : CircuitHandler
{
    private readonly ActiveUserManager _userManager;

    public BlazorConnectionTracker(ActiveUserManager userManager)
    {
        _userManager = userManager;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _userManager.UserConnected();
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _userManager.UserDisconnected();
        return Task.CompletedTask;
    }
}