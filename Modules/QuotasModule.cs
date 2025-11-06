using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using SardineBot.Database;
using SardineBot.ErrorHandling;

namespace SardineBot.Modules;

[SlashCommand("quotas", "Controlo de quotas dos membros",
    Contexts = [InteractionContextType.Guild])]
public class QuotasModule (ILogger<QuotasModule> logger, GoogleSheetsSyncService sheets): ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly GoogleSheetsSyncService _sheets = sheets;

    [SubSlashCommand("estado", "Estado das quotas de um membro.")]
    public async Task<string> VerEstadoQuotas(User membro)
    {
        string username = membro.Username;
        
        var queryResult = await new QueryRunner().QueryAsync(
            query: """
                   SELECT inscricao_inicio, inscricao_fim
                   FROM membros
                   WHERE discord_username = $username
                   """
            ,args: [("$username", username)]
            );

        if (!queryResult.Success)
        {
            throw new LoggedEntityNotFoundException("Utilizador não foi encontrado na base de dados. Será que o nick do Discord está correcto na base de dados?", logger);
        }
        
        var table = queryResult.ResultTable;
        var inscricaoFim = DateTime.Parse(table.Rows[0]["inscricao_fim"].ToString());
        TimeSpan diasAteFimInscricao = inscricaoFim - DateTime.UtcNow.Date;

        return $"\nTem inscrição válida até {inscricaoFim.ToShortDateString()}." +
               $"\nTermina em {diasAteFimInscricao.TotalDays} dias.";
    }

    /// <summary>
    /// Adicionar quotas a um membro.
    /// </summary>
    /// <param name="membro"></param>
    /// <param name="quantidade">Quantas quotas a adicionar. 1 quota = 30 dias</param>
    /// <returns></returns>
    [SubSlashCommand("adicionar", "Adicionar quotas (multiplos de 30 dias)à inscrição de um membro.")]
    public async Task<InteractionMessageProperties> AdicionarQuota(User membro, int quantidade)
    {
        var conn = new DatabaseProvider().CreateConnection();
        string username = membro.Username;

        var queryResult = await new QueryRunner().QueryAsync(
            query: """
                   UPDATE membros
                   SET inscricao_fim = DATE(inscricao_fim, '+30 days')
                   WHERE discord_username = $username
                   """
            , args: [("$username", username)]
        );

        if (!queryResult.Success)
        {
            throw new LoggedEntityNotFoundException("Não foi possivel adicionar quotas, tenta novamente daqui a um pouco.", logger);
        }

        var estadoActualizado = await VerEstadoQuotas(membro);
        return new InteractionMessageProperties()
            .WithContent($"Foram adicionados {quantidade * 30} dias a {DbHelpers.GetMembroPrimeiroUltimoNome(membro, conn).Result}" +
               $"\n{estadoActualizado}")
            .WithFlags(MessageFlags.Ephemeral);
    }
    
    /// <summary>
    /// Remover quotas a um membro.
    /// </summary>
    /// <param name="membro"></param>
    /// <param name="quantidade">Quantas quotas a remover. 1 quota = 30 dias</param>
    /// <returns></returns>
    [SubSlashCommand("remover", "remover quotas (multiplos de 30 dias)à inscrição de um membro.")]
    public async Task<InteractionMessageProperties> RemoverQuota(User membro, int quantidade)
    {
        var conn = new DatabaseProvider().CreateConnection();
        string username = membro.Username;
        
        var queryResult = await new QueryRunner().QueryAsync(
            query: """
                   UPDATE membros
                   SET inscricao_fim = DATE(inscricao_fim, '-30 days')
                   WHERE discord_username = $username
                   """
            ,args: [("$username", username)]
        );

        if (!queryResult.Success)
        {
            throw new LoggedEntityNotFoundException("Não foi possivel remover quotas, tenta novamente daqui a um pouco.", logger);
        }
        
        try
        {
            await _sheets.SyncSheetWithDbAsync(SheetnameEnum.Quotas);
        }
        catch
        {
            return new InteractionMessageProperties()
                .WithContent("Quotas actualizadas com sucesso MAS o ficheiro Sheets não foi actualizado.")
                .WithFlags(MessageFlags.Ephemeral);
        }
        
        var estadoActualizado = await VerEstadoQuotas(membro);
        return new InteractionMessageProperties()
            .WithContent($"Foram adicionados {quantidade * 30} dias a {DbHelpers.GetMembroPrimeiroUltimoNome(membro, conn).Result}" +
                         $"\n{estadoActualizado}")
            .WithFlags(MessageFlags.Ephemeral);
        
        

        

    }
}
