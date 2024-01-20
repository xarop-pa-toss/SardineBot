using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Discord.Commands;
using RestSharp;


namespace SardineBot.Commands.UrbanDictionary
{
    public class UrbanDictionary : ModuleBase<SocketCommandContext>
    {
        [Command("ud")]
        [Summary("Vai buscar a definição ao Urban Dictionary")]

        public async Task ExecuteAsync([Remainder][Summary("Termo a procurar no Urban Dictionary")] string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                await ReplyAsync($"Utilização: !sardine ud <termo a procurar>");
                return;
            }

            await ReplyAsync(phrase);
        }

        private async Task GetResponseFromUD(string phrase)
        {
            var options = new RestClientOptions("https://mashape-community-urban-dictionary.p.rapidapi.com/define?term=wat");

            var client = new RestClient(options);
            var request = new RestRequest(phrase,Method.Get);

            request.AddHeader("X-RapidAPI-Key", RapidAPI_UrbanDictionaryToken);
            request.AddHeader("X-RapidAPI-Host", "mashape-community-urban-dictionary.p.rapidapi.com");
            
            var response = await client.GetAsync<UrbanDictionaryResponse>(request);

            if (response?.List == null || !response.List.Any())
            {
                await ReplyAsync("Não foi encontrada definição para o termo procurado... aprende a escrever burro.");
            }
            else
            {
                await ReplyAsync($"_{phrase}?_");
                await ReplyAsync(response.List[0].Definition);
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
