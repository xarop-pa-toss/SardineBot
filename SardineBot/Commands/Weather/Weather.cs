using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SardineBot.Commands.Weather
{
    public class Weather : InteractionModuleBase
    {
        private readonly IConfiguration _configuration;

        public Weather(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [SlashCommand("tempo", "Busca o tempo para um certo local.")]
        public async Task WeatherAsync(
            [Choice("Hoje", "Portimão"), 
            Choice("Amanhã", "Portimão"),
            Choice("Semana", "Portimão"),
            Choice("Agora", "Portimão")] string xoxa)
        {
            IDiscordInteraction command = Context.Interaction;

            if (string.IsNullOrEmpty(xoxa))
            {
                await ReplyAsync("Utilização: /cidade <cidade a procurar>... burro");
                return;
            }
            await GetResponseFromForecaWeather(xoxa);
        }

        public async Task GetResponseFromForecaWeather(string cidade)
        {

        }
    }
}
