using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services;
using NetCord.Services.ComponentInteractions;
using SardineBot.Database;
using SardineBot.ErrorHandling;
using SardineBot.Modules.Models;

namespace SardineBot.Modules;

public class AdicionarMembroModule(IMemoryCache cache) : ApplicationCommandModule<ApplicationCommandContext>
{
    private IMemoryCache _cache = cache;
    
    [SlashCommand("criar_membro", "Criar novo membro da associação.")]
    public async Task<InteractionMessageProperties> CriarMembroMensagem()
    {   
        var callerUsername = Context.User.Username;
        _cache.Remove($"criar_membro_{callerUsername}");
        
        return new InteractionMessageProperties()
            .WithContent("Carrega nos botões para preencher os dados do novo membro")
            .AddComponents(new ActionRowProperties(new IButtonProperties[3]
            {
                new ButtonProperties("criar_membro_pag1_button", "Info 1", ButtonStyle.Primary),
                new ButtonProperties("criar_membro_pag2_button", "Info 2", ButtonStyle.Primary),
                new ButtonProperties("criar_membro_submeter_button", "Submeter", ButtonStyle.Success)
            }));
    }
}

public class CriarMembroBotoesModule(IMemoryCache cache, GoogleSheetsSyncService sheets) : ComponentInteractionModule<ButtonInteractionContext>
{
    private IMemoryCache _cache = cache;
    private readonly GoogleSheetsSyncService _sheets = sheets;
    
    [ComponentInteraction("criar_membro_pag1_button")]
    public async Task<InteractionCallbackProperties> Pag1ButtonAsync()
    {
        var callerUsername = Context.User.Username;
        _cache.TryGetValue($"criar_membro_{callerUsername}", out Membro membroCached);
            
        var callbackModal = InteractionCallback.Modal(new ModalProperties("criar_membro_pag1_modal", "Dados do novo membro")
            .AddComponents(
                new LabelProperties("Nome Completo", new TextInputProperties("nome", TextInputStyle.Short)
                {
                    Required = true,
                    Value = membroCached?.Nome
                }),
                new LabelProperties("Nº Sócio", new TextInputProperties("num_socio", TextInputStyle.Short)
                {
                    Required = false,
                    Value = membroCached?.NumSocio.ToString() ?? (DbHelpers.GetUltimoNumSocio().Result + 1).ToString()
                }),
                new LabelProperties("NIF", new TextInputProperties("nif", TextInputStyle.Short)
                {
                    Required = false,
                    MinLength = 9,
                    MaxLength = 9,
                    Value = membroCached?.Nif
                }),
                new LabelProperties("Telef.", new TextInputProperties("telef", TextInputStyle.Short)
                {
                    Required = false,
                    MaxLength = 15,
                    Value = membroCached?.Telef
                }),
                new LabelProperties("Email", new TextInputProperties("email", TextInputStyle.Short)
                {
                    Required = true,
                    Value = membroCached?.Email
                })
            ));

        return callbackModal;
    }

    [ComponentInteraction("criar_membro_pag2_button")]
    public async Task<InteractionCallbackProperties> Pag2ButtonAsync()
    {
        var callerUsername = Context.User.Username;
        _cache.TryGetValue($"criar_membro_{callerUsername}", out Membro membroCached);
        
        var callbackModal = InteractionCallback.Modal(new ModalProperties("criar_membro_pag2_modal", "Dados do novo membro")
            .AddComponents(
                new LabelProperties("Morada", new TextInputProperties("morada", TextInputStyle.Paragraph)
                {
                    Required = false,
                    Value = membroCached?.Morada
                }),
                new LabelProperties("Cod. Postal", new TextInputProperties("cod_postal", TextInputStyle.Short)
                {
                    Required = false,
                    MaxLength = 8,
                    Value = membroCached?.CodPostal
                }),
                new LabelProperties("Localidade", new TextInputProperties("localidade", TextInputStyle.Short)
                {
                    Required = false,
                    Value = membroCached?.Localidade
                }),
                new LabelProperties("Discord User", new UserMenuProperties("discord_username")
                {
                    Required = false
                })
            ));

        return callbackModal;
    }

    [ComponentInteraction("criar_membro_submeter_button")]
    public async Task<InteractionMessageProperties> MembroSubmeterButtonAsync()
    {
        var callerUsername = Context.User.Username;

        if (!_cache.TryGetValue($"criar_membro_{callerUsername}", out Membro cachedMembro))
        {
            return new InteractionMessageProperties()
                .WithContent("Formulário não foi preenchido.")
                .WithFlags(MessageFlags.Ephemeral);
        }
        
        // Validar Membro
        if (string.IsNullOrEmpty(cachedMembro.Nome)) {
            return new InteractionMessageProperties().WithContent("Nome não pode estar vazio."); 
        }
        if (!int.TryParse(cachedMembro.NumSocio, out var numSocioCheck)) {
            return new InteractionMessageProperties().WithContent("Número de sócio não é um número válido.");
        }
        if (!cachedMembro.Nif.All(char.IsDigit)) {
            return new InteractionMessageProperties().WithContent("NIF não é um número válido.");
        }
        if (!cachedMembro.Telef.All(char.IsDigit)) {
            return new InteractionMessageProperties().WithContent("Número de telefone não é um número válido.");
        }
        
        // Write to DB
        var queryResult = await new QueryRunner().QueryAsync(
            query: """
                   INSERT INTO membros(nome, num_socio, nif, telef, email, morada, cod_postal, localidade, discord_username)
                   VALUES ($nome, $num_socio, $nif, $telef, $email, $morada, $cod_postal, $localidade, $discord_username)
                   """
            ,args: [
                ("$nome", cachedMembro.Nome),
                ("$num_socio", cachedMembro.NumSocio),
                ("$nif", cachedMembro.Nif),
                ("$telef", cachedMembro.Telef),
                ("$email", cachedMembro.Email),
                ("$morada", cachedMembro.Morada),
                ("$cod_postal", cachedMembro.CodPostal),
                ("$localidade", cachedMembro.Localidade),
                ("discord_username", cachedMembro.DiscordUsername)
            ]
        );

        if (!queryResult.Success)
        {
            throw new LoggedEntityNotFoundException("Erro ao validar campos. Tenta outra vez daqui a um minuto.");
        }
        
        _cache.Remove($"criar_membro_{callerUsername}");

        try
        {
            await _sheets.SyncSheetWithDbAsync(SheetnameEnum.Detalhes);
        }
        catch
        {
            return new InteractionMessageProperties()
                .WithContent("Membro criado com sucesso MAS o ficheiro Sheets não foi actualizado.")
                .WithFlags(MessageFlags.Ephemeral);
        }

        return new InteractionMessageProperties()
            .WithContent("Membro criado com sucesso.")
            .WithFlags(MessageFlags.Ephemeral);
    }
}

public class CriarMembroModalModule(IMemoryCache cache) : ComponentInteractionModule<ModalInteractionContext>
{
    private readonly IMemoryCache _cache = cache;
    
    [ComponentInteraction("criar_membro_pag1_modal")]
    public async Task<InteractionCallbackProperties> Pag1ModalAsync()
    {
        var callerUsername = Context.User.Username;
        
        WriteMembroModalToCache();
        CacheLogger.LogObject(_cache, $"criar_membro_{callerUsername}");
        
        // Returning DeferredModifyMessage lets us return "nothing"
        return InteractionCallback.DeferredModifyMessage;
    }
    
    [ComponentInteraction("criar_membro_pag2_modal")]
    public async Task<InteractionCallbackProperties> Pag2ModalAsync()
    {
        var callerUsername = Context.User.Username;
        
        WriteMembroModalToCache();
        CacheLogger.LogObject(_cache, $"criar_membro_{callerUsername}");
        
        return InteractionCallback.DeferredModifyMessage;
    }

    private async void WriteMembroModalToCache()
    {
        var callerUsername = Context.User.Username;
        
        var membroDados = Context.Components.OfType<Label>()
            .Select(l => l.Component)
            .ToMembro();

        _cache.TryGetValue($"criar_membro_{callerUsername}", out Membro cachedMembro);
        if (cachedMembro is not null)
        {
            var mergedMembro = ObjectExtensions.Merge(membroDados, cachedMembro);
            _cache.Set($"criar_membro_{callerUsername}", mergedMembro, TimeSpan.FromMinutes(15));
        }
        else
        {
            _cache.Set($"criar_membro_{callerUsername}", membroDados, TimeSpan.FromMinutes(15));
        }
    }
}