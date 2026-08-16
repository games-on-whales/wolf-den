namespace WolfLeash.Components.Classes;

public class ActiveUserManager
{
    private int _activeUsers = 0;
    public event Action<bool>? OnActivityStateChanged;

    public void UserConnected()
    {
        // Thread-safe increment
        var currentCount = Interlocked.Increment(ref _activeUsers);
        
        if (currentCount == 1)
        {
            OnActivityStateChanged?.Invoke(true);
        }
    }

    public void UserDisconnected()
    {
        var currentCount = Interlocked.Decrement(ref _activeUsers);
        
        if (currentCount == 0)
        {
            OnActivityStateChanged?.Invoke(false);
        }
    }
}