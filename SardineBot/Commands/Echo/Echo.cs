using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SardineBot.Commands.Echo
{
    public class Echo : ModuleBase<SocketCommandContext>
    {
        public async Task ExecuteAsync(SocketSlashCommand command)
        {
            string phrase = command.Data.Options.First().Value.ToString();

            if (string.IsNullOrEmpty(phrase))
            {
                await ReplyAsync($"Utilização: /echo <texto>");
                return;
            }

            await ReplyAsync(phrase);
        }
    }
}
