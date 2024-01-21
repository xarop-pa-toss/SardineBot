using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Discord.Commands;
using Microsoft.Extensions.Configuration;
using RestSharp;


namespace SardineBot.Commands.UrbanDictionary
{
    public class UrbanDictionary : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;
        public UrbanDictionary(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Command("ud")]
        [Summary("Vai buscar a definição ao Urban Dictionary")]
        public async Task ExecuteAsync([Remainder][Summary("Termo a procurar no Urban Dictionary")] string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                await ReplyAsync($"Utilização: !sardine ud <termo a procurar>");
                return;
            }

            await GetResponseFromUD(phrase);
        }

        private async Task GetResponseFromUD(string phrase)
        {
            string UDToken = _configuration["RapidAPI_UrbanDictionaryToken"] ?? throw new Exception("Urban Dictionary token is missing. Check secrets.");

            //var options = new RestClientOptions( + phrase);
            var client = new RestClient("https://mashape-community-urban-dictionary.p.rapidapi.com/define?");

            var request = new RestRequest();

            request.AddHeader("X-RapidAPI-Key", UDToken);
            request.AddHeader("X-RapidAPI-Host", "mashape-community-urban-dictionary.p.rapidapi.com");
            request.AddParameter("term", phrase);

            var response = await client.GetAsync<UrbanDictionaryResponse>(request);


            if (response.List == null || !response.List.Any())
            {
                await ReplyAsync("Não foi encontrada definição para o que procuraste... aprende a escrever burro.");
            }
            else
            {
                await ReplyAsync($"_{phrase}?_");
                await ReplyAsync(response.List[0].Definition.ToString());
            }
        }
    }

    public class UrbanDictionaryResponse
    {
        public List<UrbanDictionaryItem>? List { get; set; }
    }

    public class UrbanDictionaryItem
    {
        public string? Definition { get; set; }
    }
}
