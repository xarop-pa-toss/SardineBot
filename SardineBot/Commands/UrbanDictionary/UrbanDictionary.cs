using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.Net.Rest;
using RestSharp;
using RestSharp.Authenticators;


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

        private async Task GetDefinitionFromUD(string phrase)
        {
            var options = new RestClientOptions("https://mashape-community-urban-dictionary.p.rapidapi.com/define?term=wat");

            var client = new RestClient(options);
            var request = new RestRequest();

            request.AddHeader("X-RapidAPI-Key", "SIGN-UP-FOR-KEY");
            request.AddHeader("X-RapidAPI-Host", "mashape-community-urban-dictionary.p.rapidapi.com");
            IRestResponse response = client.Execute(request);
        }
    }
}
