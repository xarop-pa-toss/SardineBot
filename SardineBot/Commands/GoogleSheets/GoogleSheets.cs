using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Discord.Commands;
using Microsoft.Extensions.Configuration;
using RestSharp;

using Google.Apis.Auth;
using Google.Apis.Sheets;

namespace SardineBot.Commands.GoogleSheets
{
    public class GoogleSheets : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;

        public GoogleSheets(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Command("quotas")]
        [Summary("Busca estado das quotas do user ao ficheiro Google Sheets na Drive do clube")]

        public async Task ExecuteAsync([Remainder][Summary("Buscar estado das quotas no ficheiro Google Sheets \"Lista Sócios\"")] string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                await ReplyAsync($"Utilização: !sardine ud <termo a procurar>");
                return;
            }

            await GetQuotas();
        }


    }
}
