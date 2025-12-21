using Microsoft.EntityFrameworkCore;
using GamesOnWhales;
using WolfLeash.Database.Model;

namespace WolfLeash.Database;

public static class Extensions
{
    public static DbSet<DbApp> Add(this DbSet<DbApp> db, App entity)
    {
        db.Add(new DbApp
        {
            Title = entity.Title,
            AppId = entity.Id,
            IconPngPath = entity.Icon_png_path,
            SupportHdr = entity.Support_hdr,
            StartVirtualCompositor = entity.Start_virtual_compositor,
            StartAudioServer = entity.Start_audio_server,
            Runner = new Runner
            {
                Name = entity.Runner.Name,
                Image = entity.Runner.Image,
                Mounts = entity.Runner.Mounts,
                Env = entity.Runner.Env,
                Devices = entity.Runner.Devices,
                Ports = entity.Runner.Ports,
                BaseCreateJson = entity.Runner.Base_create_json
            },
            RenderNode = entity.Render_node,
            H264_gst_pipeline = entity.H264_gst_pipeline,
            Hevc_gst_pipeline = entity.Hevc_gst_pipeline,
            Av1_gst_pipeline = entity.Av1_gst_pipeline,
            Opus_gst_pipeline = entity.Opus_gst_pipeline
        });
        
        return db;
    }

    public static App ToDto(this DbApp entity)
    {
        return new App
        {
            Title = entity.Title,
            Id = entity.AppId,
            Icon_png_path = entity.IconPngPath,
            Runner = new Wolf__config__AppDocker__tagged
            {
                Type = entity.Runner.Type,
                Name = entity.Runner.Name,
                Image = entity.Runner.Image,
                Mounts = entity.Runner.Mounts,
                Env = entity.Runner.Env,
                Devices = entity.Runner.Devices,
                Ports = entity.Runner.Ports,
                Base_create_json = entity.Runner.BaseCreateJson
            },
            Start_virtual_compositor = entity.StartVirtualCompositor ?? true,
            Start_audio_server = entity.StartAudioServer ?? true,
            Render_node = entity.RenderNode ?? "",
            H264_gst_pipeline = entity.H264_gst_pipeline ?? "",
            Hevc_gst_pipeline = entity.Hevc_gst_pipeline ?? "",
            Av1_gst_pipeline = entity.Av1_gst_pipeline ?? "",
            Opus_gst_pipeline = entity.Opus_gst_pipeline ?? "",
            Support_hdr = entity.SupportHdr ?? false,
        };
    }
}