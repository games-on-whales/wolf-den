using EntityFramework.Exceptions.Sqlite;
using Microsoft.EntityFrameworkCore;
using WolfLeash.Database.Model;

namespace WolfLeash.Database;

public class WolfLeashDbContext(DbContextOptionsBuilder<WolfLeashDbContext>? builder = null) : DbContext(CreateOptions(builder))
{
    public DbSet<DbApp> Apps { get; set; }
    public DbSet<Runner> Runners { get; set; }
    public DbSet<Client> Clients { get; set; }
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbApp>().ToTable("Apps");
        modelBuilder.Entity<Runner>().ToTable("Runners");
        modelBuilder.Entity<Client>().ToTable("Clients");
    }
    
    private static DbContextOptions<WolfLeashDbContext> CreateOptions(DbContextOptionsBuilder<WolfLeashDbContext>? builder)
    {
        if(builder is not null) return builder.Options;

        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbPath = Path.Join(path, "wolf-den");
        
        if(!Directory.Exists(dbPath))
            Directory.CreateDirectory(dbPath);
        
        dbPath = Path.Join(dbPath, "database.db");
        
        return new DbContextOptionsBuilder<WolfLeashDbContext>()
            .UseSqlite($"Data Source={dbPath};")
            .UseExceptionProcessor()
            .Options;
    }
}