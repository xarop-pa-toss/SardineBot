﻿using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Interactions;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Discord.Net;
using SardineBot.Commands;
using SardineBot.Commands.UrbanDictionary;
using SardineBot.Commands.GoogleSheets;
using SardineBot.Commands.Echo;


namespace SardineBot;

// SEE EXAMPLE PROJECT AT https://github.com/discord-net/Discord.Net/tree/dev/samples/InteractionFramework

public class Bot : IBot
{
    // LOGGING handled by LogService.LogAsync
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;
    private readonly InteractionService _handler;
    private IServiceProvider? _services;
    private readonly CommandService _commands;

    public Bot(DiscordSocketClient client, IConfiguration configuration, IServiceProvider serviceProvider, InteractionService interactionService)
    {
        _client = client;
        _configuration = configuration;
        _handler = interactionService;
        _services = serviceProvider;

        _commands = new CommandService();
        new LogService(_client, _commands);

        DiscordSocketConfig config = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };
    }

    public async Task StartAsync(IServiceProvider _services)
    {
        string botToken = _configuration["BotToken"] ?? throw new Exception("Discord Token is missing. Check secrets.");

        //_services = services;
        _client.Ready += ClientReadyAsync;
        _handler.Log += LogService.LogAsync;

        // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        // Process the InteractionCreated payloads to execute Interactions commands
        _client.InteractionCreated += HandleInteraction;
        // Process the result of the command execution.
        _handler.InteractionExecuted += HandleInteractionExecute;

        _handler.Log += InteractionServiceLog;

        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.StartAsync();
        await Task.Delay(-1);
    }

    public async Task StopAsync()
    {
        if (_client != null)
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
    }

    public async Task ClientReadyAsync()
    {
        ulong guildID = (ulong)Convert.ToDouble(_configuration["GuildID"]);

        // Register commands either on guild only or globally
        var guild = _client.GetGuild(guildID);
        await _handler.RegisterCommandsGloballyAsync();
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

    private async Task InteractionServiceLog(LogMessage logMessage)
    {
        LogService.LogAsync(logMessage);
    }
}