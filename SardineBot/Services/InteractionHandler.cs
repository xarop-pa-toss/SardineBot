using Discord;
using Discord.Interactions;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SardineBot;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;
    private readonly InteractionService _handler;

    public InteractionHandler(DiscordSocketClient client, IConfiguration configuration, IServiceProvider serviceProvider, InteractionService interactionService)
    {
        _client = client;
        _configuration = configuration;
        _services = serviceProvider;
        _handler = interactionService;
    }

    public async Task InitializeAsync()
    {
        // Trigger events to register commands only after Client has given the ready signal
        _client.Ready += ReadyAsync;
        _handler.Log += LogService.LogAsync;

        // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        // Process the InteractionCreated payloads to execute Interactions commands
        _client.InteractionCreated += HandleInteraction;

        // Also process the result of the command execution.
        _handler.InteractionExecuted += HandleInteractionExecute;
    }

    private async Task ReadyAsync()
    {
        // Register commands either on guild only or globally. Globally can take upwards of an hour to give results
        ulong guildID = (ulong)Convert.ToDouble(_configuration["GuildID"]);
        await _handler.RegisterCommandsToGuildAsync(guildID);
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new SocketInteractionContext(_client, interaction);

            // Execute the incoming command.https://github.com/discord-net/Discord.Net/blob/dev/samples/InteractionFramework/InteractionHandler.cs
            var result = await _handler.ExecuteCommandAsync(context, _services);

            // Due to async nature of InteractionFramework, the result here may always be success.
            // That's why we also need to handle the InteractionExecuted event.
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        LogService.LogAsync(new LogMessage(LogSeverity.Error, "InteractionHandler-HandleInteraction", "Error executing Slash Command async."));
                        break;
                    default:
                        break;
                }
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    private async Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, Discord.Interactions.IResult result)
    {
        if (!result.IsSuccess)
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    LogService.LogAsync(new LogMessage(LogSeverity.Error, "InteractionHandler-HandleInteractionExecute", "Error executing Slash Command async."));
                    break;
                default:
                    break;
            }
    }
}
