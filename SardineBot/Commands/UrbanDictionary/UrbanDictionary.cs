using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
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

        [SlashCommand("ud", "Busca definição do termo dado no Urban Dictionary")]
        public async Task ExecuteAsync(SocketSlashCommand command)
        {
            string termToSearch = command.Data.Options.First().Value.ToString();

            if (string.IsNullOrEmpty(termToSearch))
            {
                await ReplyAsync($"Utilização: /ud sardine ud <termo a procurar>");
                return;
            }

            await GetResponseFromUD(termToSearch);
        }

        private async Task GetResponseFromUD(string termToSearch)
        {
            string UDToken = _configuration["RapidAPI_UrbanDictionaryToken"] ?? throw new Exception("Urban Dictionary token is missing. Check secrets.");

            //var options = new RestClientOptions( + phrase);
            var client = new RestClient("https://mashape-community-urban-dictionary.p.rapidapi.com/define?");

            var request = new RestRequest();

            request.AddHeader("X-RapidAPI-Key", UDToken);
            request.AddHeader("X-RapidAPI-Host", "mashape-community-urban-dictionary.p.rapidapi.com");
            request.AddParameter("term", termToSearch);

            var response = await client.GetAsync<UrbanDictionaryResponse>(request);


            if (response.List == null || !response.List.Any())
            {
                await ReplyAsync("Não foi encontrada definição para o que procuraste... aprende a escrever burro.");
            }
            else
            {
                await ReplyAsync($"_{termToSearch}?_");
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
