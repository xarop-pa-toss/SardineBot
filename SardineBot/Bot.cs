using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Discord.Net;
using SardineBot.Commands;
using SardineBot.Commands.UrbanDictionary;
using SardineBot.Commands.GoogleSheets;
using SardineBot.Commands.Echo;
using Discord.Interactions;

namespace SardineBot;

// SEE EXAMPLE PROJECT AT https://github.com/discord-net/Discord.Net/tree/dev/samples/InteractionFramework

public class Bot : IBot
{
    // LOGGING handled by LogService.LogAsync
    private LogService _LogService;

    private readonly IConfiguration _configuration;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _handler;
    private readonly CommandService _commands;
    private IServiceProvider? _services;


    public Bot(IConfiguration configuration)
    {
        _configuration = configuration;

        DiscordSocketConfig config = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);
        _handler = new InteractionService(_client.Rest);
        _commands = new CommandService();
        _LogService = new LogService(_client, _commands);
    }


    public async Task StartAsync(IServiceProvider services)
    {
        string botToken = _configuration["BotToken"] ?? throw new Exception("Discord Token is missing. Check secrets.");

        _services = services;

        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite);

        _client.Ready += ClientReadyAsync;
        _client.SlashCommandExecuted += SlashCommandHandlerAsync;
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
        var guild = _client.GetGuild(guildID);

        SlashCommandCreator CreateCommands = new SlashCommandCreator(_client, _commands, _configuration);
    }


    private async Task InteractionHandlerAsync(SocketSlashCommand command)
    {
        // Switch statement for each of the commands created.
        switch(command.Data.Name)
        {
            case "urban":
                UrbanDictionary urbanCommand = new UrbanDictionary(_configuration);
                await urbanCommand.ExecuteAsync(command);
                break;
            case "quotas":
                GoogleSheets googleSheets = new GoogleSheets(_configuration);
                await googleSheets.ExecuteAsync(command);
                break;
            case "echo":
                Echo echo = new Echo();
                await echo.ExecuteAsync(command);
                break;
        }

        await command.RespondAsync();
    }

    private async Task TextCommandHandlerAsync(SocketMessage command)
    {
        // Ignore messages from bots
        if (command is not SocketUserMessage message || message.Author.IsBot)
        {
            return;
        }
        
        // Check if the message starts with !
        int position = 0;
        bool messageIsCommand = message.HasStringPrefix("!sardine ", ref position);

        if (messageIsCommand)
        {
            // Execute the command if it exists in the ServiceCollection
            await _commands.ExecuteAsync(
                new SocketCommandContext(_client, message), position, _serviceProvider);

            return;
        }
    }
}