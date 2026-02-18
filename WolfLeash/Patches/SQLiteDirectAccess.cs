using System;
using System.Text;
using System.Data;
using System.Data.SQLite;

namespace WolfLeash.Patches;

class SQLiteDirectAccess
{
    private SQLiteConnection sqlite;

    public SQLiteDirectAccess()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbPath = Path.Join(path, "wolf-den");
        
        if(!Directory.Exists(dbPath))
            Directory.CreateDirectory(dbPath);
        
        dbPath = Path.Join(dbPath, "database.db");
        
        sqlite = new SQLiteConnection($"Data Source={dbPath}");
    }

    public DataTable SelectQuery(string query)
    {
        SQLiteDataAdapter ad;
        var dt = new DataTable();

        try
        {
            SQLiteCommand cmd;
            sqlite.Open();
            cmd = sqlite.CreateCommand();
            cmd.CommandText = query;
            ad = new SQLiteDataAdapter(cmd);
            ad.Fill(dt);
        }
        catch (SQLiteException ex)
        {
            Console.WriteLine(ex.Message);
            return dt;
        }
        finally
        {
            sqlite.Close();
        }
        
        return dt;
    }
    
    public int TruncateTableQuery(params string[] tableNames)
    {
        var affectedRows = 0;
        try
        {
            sqlite.Open();
            foreach (var tableName in tableNames)
            {
                var cmd = sqlite.CreateCommand();
                cmd.CommandText = $"DELETE FROM {tableName};";
                affectedRows += cmd.ExecuteNonQuery();
            }
        }
        catch (SQLiteException ex)
        {
            Console.WriteLine(ex.Message);
            return affectedRows;
        }
        finally
        {
            sqlite.Close();
        }
        
        return affectedRows;
    }
}