using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using WolfLeash.Components;
using WolfLeash.Components.Classes;
using WolfLeash.Database;
using Api = WolfLeash.Components.Classes.Api;
using GamesOnWhales.Extensions;
using WolfLeash.Patches;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables("WOLF_");

builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddTransient<ColorGenerator>();
builder.Services.AddWolfApi<Api>();

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