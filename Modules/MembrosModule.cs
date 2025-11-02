using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services;
using NetCord.Services.ComponentInteractions;
using SardineBot.Database;

namespace SardineBot.Modules;

public class AdicionarMembroModule(ILogger<AdicionarMembroModule> logger): ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("criar_membro", "Criar novo membro da associação.")]
    public async Task<InteractionCallbackProperties> CriarMembro()
    {
        var callbackModal = InteractionCallback.Modal(new ModalProperties("novo_membro_modal", "Dados do novo membro")
            .AddComponents(
                new LabelProperties("Nome Completo", new TextInputProperties("nome", TextInputStyle.Short)
                {
                    Required = true
                }),
                new LabelProperties("NIF", new TextInputProperties("nif", TextInputStyle.Short)
                {
                    MinLength = 9,
                    MaxLength = 9
                }),
                new LabelProperties("Tel.", new TextInputProperties("tel", TextInputStyle.Short)
                {
                    MaxLength = 15,
                }),
                new LabelProperties("Email", new TextInputProperties("email", TextInputStyle.Short)
                {
                    Required = true
                }),
                new LabelProperties("Morada", new TextInputProperties("morada", TextInputStyle.Paragraph))
            ));

        return callbackModal;
    }
}

public class CriarMembroModalModule(ILogger<AdicionarMembroModule> logger) : ComponentInteractionModule<ModalInteractionContext>
{
    [ComponentInteraction("novo_membro_modal")]
    public async Task<InteractionMessageProperties> ModalAsync()
    {
        var textValues = Context.Components.OfType<Label>()
            .Select(l => l.Component)
            .OfType<TextInput>()
            .ToDictionary(inp => inp.CustomId, inp => inp.Value);
        
        
        
        var queryResult = await new QueryRunner().QueryAsync(
            query: """
                   SELECT inscricao_inicio, inscricao_fim
                   FROM membros
                   WHERE discord_username = $username
                   """
            ,args: [("$username", username)]
        );

    }
}