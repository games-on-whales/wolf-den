namespace WolfLeash.Extensions;

public static class LinqExtensions
{
    public static IEnumerable<GamesOnWhales.App> UniqueApps(
        this IEnumerable<GamesOnWhales.App> source,
        List<GamesOnWhales.App> filter)
    {
        foreach (var app in source)
        {
            if(filter.Any(a => a.Id == app.Id)) continue;
            yield return app;
        }
    }
}