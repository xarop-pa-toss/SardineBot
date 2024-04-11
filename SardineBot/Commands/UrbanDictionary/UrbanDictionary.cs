using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using RestSharp;


namespace SardineBot.Commands.UrbanDictionary
{
    public class UrbanDictionary : InteractionModuleBase
    {
        private readonly IConfiguration _configuration;

        public UrbanDictionary(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [SlashCommand("ud", "Busca definição de um termo no Urban Dictionary")]
        public async Task UrbanDictionaryAsync([Summary("termo", "Um termo pra eu procurar no Urban Dictionary")] string termo)
        {
            IDiscordInteraction command = Context.Interaction;

            if (string.IsNullOrEmpty(termo))
            {
                await ReplyAsync("Utilização: /ud <termo a procurar>");
                return;
            }

            await GetResponseFromUD(termo);
        }

        private async Task GetResponseFromUD(string termo)
        {
            string token = _configuration["RapidAPI_UrbanDictionaryToken"] ?? throw new Exception("Urban Dictionary token is missing. Check secrets.");

            //var options = new RestClientOptions( + phrase);
            var client = new RestClient("https://mashape-community-urban-dictionary.p.rapidapi.com/define?");
            var request = new RestRequest();

            request.AddHeader("X-RapidAPI-Key", token) ;
            request.AddHeader("X-RapidAPI-Host", "mashape-community-urban-dictionary.p.rapidapi.com");
            request.AddParameter("term", termo);

            var response = await client.GetAsync<UrbanDictionaryResponse>(request);

            if (response.List == null || !response.List.Any())
            {
                await ReplyAsync("Não foi encontrada definição para o que procuraste... aprende a escrever burro.");
            }
            else
            {
                await ReplyAsync($"_{termo}?_");
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
