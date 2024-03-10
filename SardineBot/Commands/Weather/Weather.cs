using Discord;
using Discord.Interactions;
using Discord.Net.Rest;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SardineBot.Commands.Weather
{
    public class Weather : InteractionModuleBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _token;

        public Weather(IConfiguration configuration)
        {
            _configuration = configuration;
            _token = _configuration["RapidAPI_ForecaWeatherToken"] ?? throw new Exception("Foreca Weather token is missing. Check secrets.");
        }

        [SlashCommand("tempo", "Busca o tempo para um certo local.")]
        public async Task WeatherAsync(string local, TempoTipos tempo)
        {
            IDiscordInteraction command = Context.Interaction;

            await GetResponseFromForecaWeather(local, tempo);
        }

        public async Task GetResponseFromForecaWeather(string local, TempoTipos tempo)
        {
            // Get Location ID from API. Used for all types of forecast calls
            string? locationID = await GetLocationIDFromForecaWeather(local);

            if (string.IsNullOrEmpty(locationID))
            {
                await ReplyAsync("Local inserido não foi encontrado.");
                return;
            }

            // Time

            RestClient client = null;
            switch (tempo)
            {
                case TempoTipos.Agora:
                    client = new RestClient($"https://foreca-weather.p.rapidapi.com/forecast/15minutely/{locationID}?alt=0&tempunit=C&windunit=KMH&periods=12&history=1&dataset=standard&lang=pt");
                    break;
                case TempoTipos.Hoje:
                    client = new RestClient($"https://foreca-weather.p.rapidapi.com/forecast/hourly/{locationID}?alt=0&tempunit=C&windunit=KMH&periods=24&dataset=standard&lang=pt");
                    break;
                case TempoTipos.Amanhã:
                    client = new RestClient($"https://foreca-weather.p.rapidapi.com/forecast/hourly/{locationID}?alt=0&tempunit=C&windunit=KMH&periods=48&dataset=standard&lang=pt");
                    break;
                case TempoTipos.Proximos7Dias:
                    client = new RestClient($"https://foreca-weather.p.rapidapi.com/forecast/daily/{locationID}?alt=0&tempunit=C&windunit=KMH&periods=12&dataset=standard&lang=pt");
                    break;
                default:
                    break;
            }

            if (client == null) 
            {
                await LogService.LogAsync(new LogMessage(LogSeverity.Error, "Weather.cs", "Error on Weather client API call."));
                return;
            }

            RestRequest request = new RestRequest();
            request.AddHeader("X-RapidAPI-Key", _token);
            request.AddHeader("X-RapidAPI-Host", "foreca-weather.p.rapidapi.com");
            var response = await client.GetAsync(request);

        }

        public async Task<string> GetLocationIDFromForecaWeather(string local)
        {
            var client = new RestClient($"https://foreca-weather.p.rapidapi.com/location/search/{local}?lang=pt");
            var request = new RestRequest();
            request.AddHeader("X-RapidAPI-Key", _token);
            request.AddHeader("X-RapidAPI-Host", "foreca-weather.p.rapidapi.com");

            var response = await client.GetAsync(request);
            
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var jsonObj = JObject.Parse(response.Content);
                string? locationID = jsonObj["locations"]?[0]?["id"]?.ToString();
                return locationID;
            }

            return "";
        }


        public enum TempoTipos
        {
            [ChoiceDisplay("Agora (próximas 3 horas em intervalos de 15 minutos)")]
            Agora,
            [ChoiceDisplay("Hoje (cada hora)")]
            Hoje,
            [ChoiceDisplay("Amanhã (cada hora)")]
            Amanhã,
            [ChoiceDisplay("7 dias (cada dia)")]
            Proximos7Dias
        }
    }
}
