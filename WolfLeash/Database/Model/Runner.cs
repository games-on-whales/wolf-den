using System.ComponentModel.DataAnnotations;

namespace WolfLeash.Database.Model;

public class Runner
{
    [Key]
    public int Id { get; set; }

    public int DbAppId { get; set; }
    public DbApp DbApp { get; set; }
    
    [Required, StringLength(64)]
    public string Name { get; set; } = null!;
    [Required, StringLength(128)]
    public string Image { get; set; } = null!;
    [StringLength(10)]
    public string Type { get; set; } = "docker";
    public ICollection<string> Mounts { get; set; } = [];
    public ICollection<string> Env { get; set; } = [];
    public ICollection<string> Devices { get; set; } = [];
    public ICollection<string> Ports { get; set; } = [];

    [Required, StringLength(256)]
    public string BaseCreateJson { get; set; } = """
                                                  {
                                                  "HostConfig": {
                                                          "IpcMode": "host",
                                                          "Privileged": false,
                                                          "CapAdd": ["NET_RAW", "MKNOD", "NET_ADMIN"],
                                                          "DeviceCgroupRules": ["c 13:* rmw", "c 244:* rmw"]
                                                      }
                                                  }
                                                  """;
}