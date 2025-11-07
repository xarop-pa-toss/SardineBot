using Microsoft.Data.Sqlite;
using NetCord;
using SardineBot.ErrorHandling;
namespace SardineBot.Database;

public static class DbHelpers
{
    public async static Task<string> GetMembroPrimeiroUltimoNome(User membro, SqliteConnection connection = null)
    {
        var result = await new QueryRunner().QueryAsync(
            """
            SELECT nome
            FROM membros
            WHERE discord_username = $username 
            """
            , [("$username", membro.Username)]
            , connection
        );

        var nomeCompleto = result.ResultTable.Rows[0]["nome"].ToString() ?? string.Empty;
        var partes = nomeCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length == 0)
        {
            return string.Empty;
        }

        return $"{partes.First()} {partes.Last()}";
    }

    public async static Task<int> GetUltimoNumSocio(SqliteConnection connection = null)
    {
        var result = await new QueryRunner().QueryAsync(
            """
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