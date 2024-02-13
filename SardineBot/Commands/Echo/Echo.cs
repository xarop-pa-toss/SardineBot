using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SardineBot.Commands.Echo
{
    public class Echo : InteractionModuleBase
    {
        [SlashCommand("echo","Repeats back text")]
        public async Task EchoAsync([Summary("frase", "Uma frase para eu dizer de volta")] string frase)
        {
            if (string.IsNullOrEmpty(frase))
            {
                await ReplyAsync($"Utilização: echo <texto>");
                return;
            }

            await ReplyAsync(frase);
        }
    }
}
