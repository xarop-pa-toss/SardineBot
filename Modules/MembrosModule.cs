using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
namespace SardineBot;

public class MembrosModule
{
    [SubSlashCommand("criar", "Criar novo membro.")]
    public async Task<string> CriarMembro()
    {
        var callback = InteractionCallback.Message("This is InteractionCallback message.");

        return "";
    }
}