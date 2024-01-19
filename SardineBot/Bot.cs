using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SardineBot
{
    internal class Bot : IBot
    {
        private ServiceProvider? _serviceProvider;

        private readonly ILogger<Bot> _logger;
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public Bot(
            ServiceProvider? serviceProvider, ILogger<Bot> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            DiscordSocketConfig socketConfig = new()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(socketConfig);
            _commands = new CommandService();
        }

        public async Task StartAsync(ServiceProvider services)
        {
            string botToken = _configuration["DiscordToken"] ?? throw new Exception("Bot Token is not present. Check project secrets.");

            _logger.LogInformation($"SardineBot is salted and ready to grill!");
            
            _serviceProvider = services;

            await _commands.AddModuleAsync(Assembly.GetExecutingAssembly().GetType(), _serviceProvider);
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("SardineBot was too delicious for this world. Cya later!");

            if (_client != null)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
            }
        }

        private async Task HandleCommandAsync(SocketMessage input)
        {
            // Ignore message from any bot
            if (input is not SocketUserMessage message || message.Author.IsBot)
            {
                return;
            }

            // Log message for debug
            _logger.LogInformation($"{DateTime.Now.ToShortTimeString()} - {message.Author}: {message.Content}");

            // Commands should start with ! (exclamation mark). Anything else is ignore
            int charIndex = 0;
            bool messageIsCommand = message.HasCharPrefix('!', ref charIndex);

            if (messageIsCommand)
            {
                // Execute command if it matches up with any in the ServiceCollection
                await _commands.ExecuteAsync(
                    new SocketCommandContext(_client, message),
                    charIndex,
                    _serviceProvider);
            }
        }
    }
}
