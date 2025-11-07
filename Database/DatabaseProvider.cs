using Microsoft.Data.Sqlite;
namespace SardineBot.Database;

public class DatabaseProvider
{

    public SqliteConnection CreateConnection()
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "sardinebot.db");
        return new SqliteConnection($"Data Source={Path.GetFullPath(dbPath)}");
    }
}