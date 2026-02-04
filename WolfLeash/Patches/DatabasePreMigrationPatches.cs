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
        var clientsExists = _directAccess
            .SelectQuery("SELECT * FROM __EFMigrationsHistory WHERE MigrationId='20251118200700_AddedClients';");
        if (clientsExists.Rows.Count <= 0) return; //Clients not yet implemented in this version.
        
        var data = _directAccess
            .SelectQuery("SELECT * FROM __EFMigrationsHistory WHERE MigrationId='20260202213438_EnableCascadingDeleteForRunner';");
        if (data.Rows.Count > 0) return; //Migration already applied.
        
        _directAccess.TruncateTableQuery("Apps", "Runners");
    }
}