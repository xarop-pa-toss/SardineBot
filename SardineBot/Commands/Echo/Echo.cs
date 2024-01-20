using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SardineBot.Commands.Echo
{
    public class Echo : ModuleBase<SocketCommandContext>
    {
        [Command("echo")]
        [Summary("Tu escreves, eu repito")]

        public async Task ExecuteAsync([Remainder][Summary("Algum texto")] string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                await ReplyAsync($"Utilização: !sardine echo <texto>");
                return;
            }

            await ReplyAsync(phrase);
        }
    }
}
