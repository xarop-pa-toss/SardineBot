using Discord;
using Discord.Interactions;
using Discord.Net.Rest;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickChart;

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
        public async Task WeatherAsync(string local, [Summary("tempo")]TempoTipos timeframe)
        {
            IDiscordInteraction command = Context.Interaction;
            await GetResponseFromForecaWeather(local, timeframe);
        }

        public async Task GetResponseFromForecaWeather(string local, TempoTipos timeframe)
        {
            // Get Location ID from API. Used for all types of forecast calls
            string localBaseURL = $"https://foreca-weather.p.rapidapi.com/location/search/{local}?lang=pt";
            JObject? locationJson = await GetJsonObjectFromForecaWeather(localBaseURL);

            string? locationID = locationJson["locations"]?[0]?["id"]?.ToString();
            if (string.IsNullOrEmpty(locationID))
            {
                await LogService.LogAsync(new LogMessage(LogSeverity.Error, "Weather.cs - GetJsonObjectFromForecaWeather", "Could not get JSON object from API."));
            }



            // Get Forecast data
            string forecastBaseURL = "";

            switch (timeframe)
            {
                case TempoTipos.Agora:
                    //Nowcast
                    forecastBaseURL = $"https://foreca-weather.p.rapidapi.com/forecast/15minutely/{locationID}?alt=0&tempunit=C&windunit=KMH&periods=12&history=1&dataset=standard&lang=pt";
                    break;
                case TempoTipos.Hoje:
                    //Hourly
                    forecastBaseURL = $"https://foreca-weather.p.rapidapi.com/forecast/hourly/{locationID}?alt=0&tempunit=C&windunit=KMH&periods=24&dataset=standard&lang=pt";
                    break;
                case TempoTipos.Amanhã:
                    //Hourly
                    forecastBaseURL = $"https://foreca-weather.p.rapidapi.com/forecast/hourly/{locationID}?alt=0&tempunit=C&windunit=KMH&periods=48&dataset=standard&lang=pt";
                    break;
                case TempoTipos.Proximos7Dias:
                    //Daily
                    forecastBaseURL = $"https://foreca-weather.p.rapidapi.com/forecast/daily/{locationID}?alt=0&tempunit=C&windunit=KMH&periods=12&dataset=standard&lang=pt";
                    break;
                default:
                    break;
            }

            // 'forecastJson' holds all information. A JArray is made to hold the info inside the first "layer". Then another JObject is made for each 0, 1, 2 etc
            JObject? forecastJson = await GetJsonObjectFromForecaWeather(localBaseURL);

            JArray? forecastArray = (JArray)forecastJson?["forecast"];
            if (forecastArray == null) 
            { 
                LogService.LogAsync(new LogMessage(LogSeverity.Error, "Weather.cs", "forecastArray is empty. API probably replied with no valid data.")); 
            }


            foreach (JObject forecastObj in forecastArray)
            {
                // Daily forecasts use a different "date" instead of "time" for their timeframe values
                // "time" is in format yyyy-MM-dd'T'hh:mm'Z' but "date" is just yyyy-MM-dd
                string time = "";                
                if (timeframe != TempoTipos.Proximos7Dias) {
                    time = forecastObj["time"].ToString().Substring(0, 10);
                }

                string date = forecastObj["time"].ToString().Substring(11, 5);
                string visualForecast = forecastObj["symbolPhrase"].ToString();
                string temperature = forecastObj["temperature"].ToString();
                string realFeel = forecastObj["feelsLikeTemp"].ToString();
                string humidity = forecastObj["relHumidity"].ToString();
                string precipitationProb = forecastObj["precipProb"].ToString();
                string windSpeed = forecastObj["windSpeed"].ToString();
                string windDirection = forecastObj["windDirString"].ToString();


            }
        }

        public async Task<JObject?> GetJsonObjectFromForecaWeather(string baseURL)
        {
            var client = new RestClient(baseURL);
            var request = new RestRequest();
            request.AddHeader("X-RapidAPI-Key", _token);
            request.AddHeader("X-RapidAPI-Host", "foreca-weather.p.rapidapi.com");

            var response = await client.GetAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject? jsonObj = JObject.Parse(response.Content);
                return jsonObj;
            }

            return null;
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
