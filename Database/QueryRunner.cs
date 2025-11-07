using System.Data;
using Microsoft.Data.Sqlite;
using SardineBot.Database.Models;
namespace SardineBot.Database;

public class QueryRunner
{
    /// <summary>
    ///     If connection parameter is provided, it will not be closed regardless of method outcome.
    ///     If no connection is given, a new one is created which is disposed on return.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="query"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public async Task<QueryResult> QueryAsync(string query, (string queryVar, string value)[]? args = null, SqliteConnection? connection = null)
    {
        if (connection is null)
        {
            using var conn = new DatabaseProvider().CreateConnection();
            await conn.OpenAsync();
            return await RunQueryAsync(conn, query, args);
        }
        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
        {
            await connection.OpenAsync();
        }
        return await RunQueryAsync(connection, query, args);
    }

    private async Task<QueryResult> RunQueryAsync(SqliteConnection conn, string query, (string queryVar, string value)[]? args)
    {
        // Command builder
        await using var command = conn.CreateCommand();
        command.CommandText = query;

        if (args is not null)
        {
            foreach (var arg in args)
            {
                command.Parameters.AddWithValue(arg.queryVar, arg.value);
            }
        }

        // Since only SELECT queries return rows, a case must be opened for ExecuteQuery and ExecuteNonQuery
        var isSelect = query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);

        if (isSelect)
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                return new QueryResult
                {
                    Success = false,
                    ErrorMessage = "No rows found."
                };
            }

            var resultTable = new DataTable();
            resultTable.Load(reader);
            return new QueryResult
            {
                Success = true,
                ResultTable = resultTable
            };
        }

        // Non-Select dont return a table, just amount of affected rows
        var affected = await command.ExecuteNonQueryAsync();
        return new QueryResult
        {
            Success = affected > 0
        };
    }
}