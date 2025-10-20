using NetCord;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace SardineBot;

[SlashCommand("quotas", "Comandos para quotas")]
public class GuildCommandsModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("adicionar", "Adicionar quota a membro")]
    public async Task<string> AdicionarQuota(string nome)
    {
        string nomeMembro = "";
        DateTime inscricaoFim = new DateTime(2025, 10, 30);
        TimeSpan diasAteFimInscricao = inscricaoFim.Date - DateTime.UtcNow.Date;

        string Channels(User? user = null) => $"Adicionada quota a {nomeMembro}." +
                                                     $"\nTem inscrição até {inscricaoFim}." +
                                                     $"\nInscrição termina em {diasAteFimInscricao}";

        return diasAteFimInscricao.Days.ToString();
    }

    public string ChannelsA() => $"Channels: {Context.Guild!.Channels.Count}";    
    
    [SubSlashCommand("remover", "Remover quota a membro")]
    public string Channels4() => $"Channels: {Context.Guild!.Channels.Count}";    
    
    [SubSlashCommand("operation", "Get stats for chosen operation")]
    public string Channels3(User? user = null) => $"Username is {user.Username} and it is pretty noice";

    [SubSlashCommand("totals", "Get total stats from all players on all operations")]
    public string Channels2() => $"Channels: {Context.Guild!.Channels.Count}";
}

[SubSlashCommand("stats_for_operation", "Get stats for chosen operation")]
public class GuildNameModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("get", "Get guild name")]
    public string GetName() => $"Name: {Context.Guild!.Name}";

    [RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
    [RequireBotPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
    [SubSlashCommand("set", "Set guild name")]
    public async Task<string> SetNameAsync(string name)
    {
        var guild = Context.Guild!;
        await guild.ModifyAsync(g => g.Name = name);
        return $"Name: {guild.Name} -> {name}";
    }
}
