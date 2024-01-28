using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Discord.Net;

namespace SardineBot
{
    public class Bot : IBot
    {
        // LOGGING handled by LogService.LogAsync
        private ServiceProvider? _serviceProvider;
        
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private LogService _LogService;
      
        public Bot(IConfiguration configuration)
        {
            _configuration = configuration;

            DiscordSocketConfig config = new()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);
            _commands = new CommandService();
            _LogService = new LogService(_client, _commands);

            CreateCommands();
        }


        public async Task StartAsync(ServiceProvider services)
        {
            string botToken = _configuration["BotToken"] ?? throw new Exception("Discord Token is missing. Check secrets.");

            _serviceProvider = services;

            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

            await _client.LoginAsync(TokenType.Bot, botToken);
            await _client.StartAsync();

            _client.SlashCommandExecuted += HandleSlashCommandAsync;
        }

        public async Task StopAsync()
        {
            if (_client != null)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
            }
        }


        private void CreateCommands()
        {
            CreateGlobalCommand("urban", "Vai buscar definição ao Urban Dictionary", true);
            CreateGlobalCommand("quotas", "Tás com a consciência pesada né caloteiro?!", true);
        }

        private async Task CreateGlobalCommand(string name, string description, bool withDMPermission)
        {
            //https://discordnet.dev/guides/int_basics/application-commands/slash-commands/creating-slash-commands.html
            // Create SlashCommandBuilder followed by client.CreateApplicationCommandAsync in a try-catch.
            // If it fails, it will create a ApplicationCommandException

            var globalCommand = new SlashCommandBuilder();
            globalCommand.WithName(name);
            globalCommand.WithDescription(description);
            globalCommand.WithDMPermission(true);

            try
            {
                await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
            }
            catch (ApplicationCommandException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
            }
        }

        private async Task CreateGuildCommand(string name, string description, string guildName, bool withDMPermission)
        {
            //https://discordnet.dev/guides/int_basics/application-commands/slash-commands/creating-slash-commands.html
            // Create SlashCommandBuilder followed by client.CreateApplicationCommandAsync in a try-catch.
            // If it fails, it will create a ApplicationCommandException

            var guild = _client.GetGuild((ulong)Convert.ToDouble(_configuration["GuildID"]));

            var guildCommand = new SlashCommandBuilder();
            guildCommand.WithName(name);
            guildCommand.WithDescription(description);
            guildCommand.WithDMPermission(true);

            try
            {
                await guild.CreateApplicationCommandAsync(guildCommand.Build());
            }
            catch (ApplicationCommandException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
            }
        }

        private async Task HandleSlashCommandAsync(SocketSlashCommand command)
        {
            try
            {
                
            }
            await command.RespondAsync()
        }

        private async Task HandleCommandAsync(SocketMessage command)
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
}