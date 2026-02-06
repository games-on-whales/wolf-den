namespace WolfLeash.Patches;

public class DatabasePreMigrationPatches
{
    private SQLiteDirectAccess _directAccess = new SQLiteDirectAccess();
    
    public void Execute()
    {
        PatchFor_EnableCascadingDeleteForRunner();
    }

    private void PatchFor_EnableCascadingDeleteForRunner()
    {
        var appsExist = _directAccess
            .SelectQuery("SELECT * FROM __EFMigrationsHistory WHERE MigrationId='20251115084840_InitialCreate';");
        if (appsExist.Rows.Count <= 0) return; //Apps not yet implemented in this version, Basically a new installation.
        
        var data = _directAccess
            .SelectQuery("SELECT * FROM __EFMigrationsHistory WHERE MigrationId='20260202213438_EnableCascadingDeleteForRunner';");
        if (data.Rows.Count > 0) return; //Migration already applied.
        
        _directAccess.TruncateTableQuery("Apps", "Runners");
    }
}