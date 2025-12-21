using System.Diagnostics.Contracts;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using WolfLeash.Database;
using WolfLeash.Database.Model;

namespace WolfLeash.Components.Classes;

public static class AppExtensions
{
    public static async Task Delete(this GamesOnWhales.App app, GamesOnWhales.WolfApi api, WolfLeashDbContext context)
    {
        if (api.Profiles is null)
        {
            await api.UpdateProfiles();
        }
        
        bool found = false;
        foreach (var owner in api.Profiles)
        { 
            var del = owner.Apps.FirstOrDefault(a => a.Identical(app));
            if(del is null)
                continue;
            
            found = true;
            owner.Apps.Remove(del);
            owner.Apps.ToList().ForEach(a => Console.WriteLine(a.Title));
            var a = await api.UpdateProfile(owner.Id, owner);
            Console.WriteLine($"{a.Success}");
        }

        if (!found)
        {
            await app.Map(context).Delete(context);
        }
    }

    public static async Task Delete(this DbApp app, WolfLeashDbContext context)
    {
        context.Apps.Remove(app);
        await context.SaveChangesAsync();
    }
    
    [Pure]
    public static bool Identical(this GamesOnWhales.App first, GamesOnWhales.App second)
    {
        return first.Title == second.Title &&
            first.Icon_png_path == second.Icon_png_path &&
            first.Runner.Base_create_json == second.Runner.Base_create_json &&
            first.Runner.Name == second.Runner.Name &&
            first.Runner.Image == second.Runner.Image &&
            first.Runner.Type == second.Runner.Type &&
            first.Runner.Devices.SequenceEqual(second.Runner.Devices) &&
            first.Runner.Mounts.SequenceEqual(second.Runner.Mounts) &&
            first.Runner.Env.SequenceEqual(second.Runner.Env) &&
            first.Runner.Ports.SequenceEqual(second.Runner.Ports);
    }

    public static async Task Save(this GamesOnWhales.App app, WolfLeashDbContext context)
    {
        var dbApp = app.Map(context);
        context.Apps.Add(dbApp);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (UniqueConstraintException e)
        {
            Console.WriteLine(e);
        }
    }

    public static DbApp Map(this GamesOnWhales.App app, WolfLeashDbContext context)
    {
        var dbApp = context.Apps
            .Include(a => a.Runner)
            .ToList()
            .FirstOrDefault(a => a.Title == app.Title &&
                a.IconPngPath == app.Icon_png_path &&
                a.Runner.BaseCreateJson == app.Runner.Base_create_json &&
                a.Runner.Name == app.Runner.Name &&
                a.Runner.Image == app.Runner.Image &&
                a.Runner.Type == app.Runner.Type &&
                a.Runner.Devices.SequenceEqual(app.Runner.Devices) &&
                a.Runner.Mounts.SequenceEqual(app.Runner.Mounts) &&
                a.Runner.Env.SequenceEqual(app.Runner.Env) &&
                a.Runner.Ports.SequenceEqual(app.Runner.Ports)
            );
        
        dbApp ??= new DbApp()
        {
            AppId = app.Id,
            Av1_gst_pipeline = app.Av1_gst_pipeline,
            H264_gst_pipeline = app.H264_gst_pipeline,
            Hevc_gst_pipeline = app.Hevc_gst_pipeline,
            Opus_gst_pipeline = app.Opus_gst_pipeline,
            IconPngPath = app.Icon_png_path,
            RenderNode = app.Render_node,
            Title = app.Title,
            Runner = new Runner()
            {
                BaseCreateJson = app.Runner.Base_create_json,
                Devices = app.Runner.Devices,
                Mounts = app.Runner.Mounts,
                Env = app.Runner.Env,
                Ports = app.Runner.Ports,
                Image =  app.Runner.Image,
                Type = app.Runner.Type,
                Name = app.Runner.Name
            }
        };
        return dbApp;
    }
}