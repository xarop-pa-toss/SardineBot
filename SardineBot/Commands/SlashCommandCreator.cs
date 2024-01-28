using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

using Microsoft.Extensions.Configuration;

using Discord;

namespace SardineBot.Commands
{
    internal class SlashCommandCreator
    {
        private readonly IConfiguration _configuration;

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public SlashCommandCreator(DiscordSocketClient client, CommandService commands, IConfiguration configuration)
        {
            _configuration = configuration;
            _client = client;
            _commands = commands;

            CreateCommands();
        }

        private void CreateCommands()
        {
            //https://discordnet.dev/guides/int_basics/application-commands/slash-commands/creating-slash-commands.html
            // Create SlashCommandBuilder followed by client.CreateApplicationCommandAsync in a try-catch.
            // If it fails, it will create a ApplicationCommandException
            SlashCommandBuilder command = new SlashCommandBuilder();
            List<string> failedCommands = new List<string>();

            ulong guildID = (ulong)Convert.ToDouble(_configuration["GuildID"]);
            var guild = _client.GetGuild(guildID);


            #region /UrbanDictionary - ud
            command.WithName("ud");
            command.WithDescription("Vai buscar definição ao Urban Dictionary");
            command.WithDMPermission(true);
            command.AddOption("termo", ApplicationCommandOptionType.String, "O termo a procurar", isRequired: true);

            try
            {
                guild.CreateApplicationCommandAsync(command.Build());
                //_client.CreateGlobalApplicationCommandAsync(command.Build());
            }
            catch (ApplicationCommandException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
                failedCommands.Add(json);
                Console.WriteLine(json);
            }
            #endregion

            #region GoogleSheets - quotas
            command.WithName("quotas");
            command.WithDescription("Tás com a consciência pesada né caloteiro?!");
            command.WithDMPermission(true);

            try
            {
                guild.CreateApplicationCommandAsync(command.Build());
                //_client.CreateGlobalApplicationCommandAsync(command.Build());
            }
            catch (ApplicationCommandException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
                failedCommands.Add(json);
                Console.WriteLine(json);
            }
            #endregion

            #region Echo - echo
            command.WithName("echo");
            command.WithDescription("Tu falas, eu repito. Só isso 😃");
            command.WithDMPermission(false);

            try
            {
                guild.CreateApplicationCommandAsync(command.Build());
                //_client.CreateGlobalApplicationCommandAsync(command.Build());
            }
            catch (ApplicationCommandException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
                failedCommands.Add(json);
                Console.WriteLine(json);
            }
            #endregion
        }
    }
}
