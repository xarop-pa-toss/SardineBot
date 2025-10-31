using Microsoft.Data.Sqlite;
namespace SardineBot.DatabaseHandlers;

public class DatabaseProvider
{
    public DatabaseProvider()
    {
    }

    public SqliteConnection CreateConnection()
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "sardinebot.db");
        return new SqliteConnection($"Data Source={Path.GetFullPath(dbPath)}");
    }
    
}