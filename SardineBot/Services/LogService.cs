using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SardineBot
{
    public class LogService
    {
        public LogService(DiscordSocketClient client, CommandService command)
        {
            client.Log += LogAsync;
            command.Log += LogAsync;
        }

        internal static Task LogAsync(LogMessage logMessage)
        {
            if (logMessage.Exception is CommandException cmdException)
            {
                Console.WriteLine(
                    $"EXCEPTION:Command/{logMessage.Severity} - {cmdException.Command.Aliases.First()}" +
                    $" falhou ao executar no canal {cmdException.Context.Channel}");
            }
            else
            {
                Console.WriteLine($"EXCEPTION:General/{logMessage.Severity} - {logMessage}");
            }

            return Task.CompletedTask;
        }
    }
}