using Microsoft.EntityFrameworkCore;
using WolfLeash.Components.Classes;
using WolfLeash.Database;
using Api = WolfLeash.Components.Classes.Api;
using GamesOnWhales.Extensions;
using GamesOnWhales.SSE;
using WolfLeash.Patches;
using App = WolfLeash.Components.App;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables("WOLF_");

builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLogging(configure =>
{
    configure.AddConsole();
#if DEBUG
    configure.AddFilter("GamesOnWhales", LogLevel.Debug);
#endif
});

builder.Services.AddTransient<ColorGenerator>();

builder.Services.AddTransient<ISseEventHandler, PairSignalEventHandler>();
builder.Services.AddWolfApi<Api>();

builder.Services.AddSingleton<DefaultAppLoader>();

var migrationPatcher = new DatabasePreMigrationPatches();
migrationPatcher.Execute();

builder.Services.AddDbContext<WolfLeashDbContext>();

builder.Services.AddBlazorBootstrap();
builder.Services.AddScoped<EventLogger>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    
    // Update Database
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WolfLeashDbContext>();
    await db.Database.MigrateAsync();
}
else
{
    // Recreate Database for Development
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WolfLeashDbContext>();
    //db.Database.EnsureDeleted();
    await db.Database.MigrateAsync();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();