using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using NetCord;
using NetCord.Rest;
using SardineBot.ErrorHandling;
namespace SardineBot.Database;

public static class DbHelpers
{
    public static async Task<string> GetMembroPrimeiroUltimoNome(User membro, SqliteConnection connection = null)
    {
        var result = await new QueryRunner().QueryAsync(
            query: """
                   SELECT nome
                   FROM membros
                   WHERE discord_username = $username 
                   """
            , args: [("$username", membro.Username)]
            , connection: connection
        );

        var nomeCompleto = result.ResultTable.Rows[0]["nome"].ToString() ?? string.Empty;
        var partes = nomeCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length == 0)
        {
            return string.Empty;
        }

        return $"{partes.First()} {partes.Last()}";
    }

    public static async Task<int> GetUltimoNumSocio(SqliteConnection connection = null)
    {
        var result = await new QueryRunner().QueryAsync(
            query: """
                   SELECT max(num_socio)
                   FROM membros
                   """
            , connection: connection
        );

        if (!result.Success)
        {
            throw new LoggedEntityNotFoundException("Erro ao buscar último número de sócio.");
        }

        return Convert.ToInt32(result.ResultTable.Rows[0][0].ToString());
    }
}