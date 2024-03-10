using Discord;
using Discord.Interactions;
using Discord.Net.Rest;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickChart;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SardineBot.Commands.Weather;

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
    public async Task WeatherAsync(string local, [Summary("tempo")] TempoTipos timeframe)
    {
        IDiscordInteraction command = Context.Interaction;
        await GetResponseFromForecaWeather(local, timeframe);
    }

    public async Task GetResponseFromForecaWeather(string local, TempoTipos timeframe)
    {
        #region Get Location ID from API. Used for all types of forecast calls
        string localBaseURL = $"https://foreca-weather.p.rapidapi.com/location/search/{local}?lang=pt";
        JObject? locationJson = await GetJsonObjectFromForecaWeather(localBaseURL);

        string? locationID = locationJson["locations"]?[0]?["id"]?.ToString();
        if (string.IsNullOrEmpty(locationID))
        {
            await LogService.LogAsync(new LogMessage(LogSeverity.Error, "Weather.cs - GetJsonObjectFromForecaWeather", "Could not get JSON object from API."));
        }
        #endregion

        #region Get Forecast data
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
        #endregion

        #region Transform data into graph with QuickChart.io
        // call TransformForecast
        JArray? forecastArray = (JArray)forecastJson?["forecast"];
        if (forecastArray == null)
        {
            LogService.LogAsync(new LogMessage(LogSeverity.Error, "Weather.cs", "forecastArray is empty. API probably replied with no valid data."));
        }


        foreach (JObject forecastObj in forecastArray)
        {
            // Daily forecasts use a different "date" instead of "time" for their timeframe values
            // "time" is in format yyyy-MM-dd'T'hh:mm'Z' but "date" is just yyyy-MM-dd

            string timeFull = forecastObj["time"].ToString();

            string time = "", date = "";
            if (timeframe != TempoTipos.Proximos7Dias) {
                time = forecastObj["time"].ToString().Substring(0, 10);
                date = timeFull.Substring(11, 5);
            }

            string visualForecast = forecastObj["symbolPhrase"].ToString();
            string temperature = forecastObj["temperature"].ToString();
            string realFeel = forecastObj["feelsLikeTemp"].ToString();
            string humidity = forecastObj["relHumidity"].ToString();
            string precipitationProb = forecastObj["precipProb"].ToString();
            string windSpeed = forecastObj["windSpeed"].ToString();
            string windDirection = forecastObj["windDirString"].ToString();

            #endregion
        }
    }

    public async Task<dynamic> CreateGraphFromForecastJson(JArray forecastArray, TempoTipos timeframe)
    {
        List<string> timeList = new List<string>();
        List<string> temperatureList = new List<string>();
        List<string> realFeelList = new List<string>();
        foreach (JObject forecastObj in forecastArray)
        {
            string timeStr = timeframe == TempoTipos.Proximos7Dias ? forecastObj["time"].ToString() : forecastObj["date"].ToString();
            timeList.Add(timeStr);

            temperatureList.Add(forecastObj["temperature"].ToString());
            realFeelList.Add(forecastObj["feelsLikeTemp"].ToString());


        }
        string timeListStr = string.Join(", ", timeList);
        string temperatureListStr = string.Join(", ", temperatureList);
        string realFeelListStr = string.Join(", ", realFeelList);


        Chart forecastChart = new Chart()
        {
            Width = 500,
            Height = 300,
            Config = "{" +
                "type: 'bar'," +
                "data: {" +
                   $"labels: [{timeListStr}]," +
                    "datasets: [" +
                    "{" +
                        "type: 'bar'," +
                        "label: 'Temperatura'," +
                        "backgroundColor: 'rgba(255, 99, 132, 0.5)'," +
					    "borderColor: 'rgb(255, 99, 132)'," +
					    $"data: [{temperatureListStr}]
						      },
						      {
						        "type": "bar",
						        "label": "Dataset 2",
						        "backgroundColor": "rgba(54, 162, 235, 0.5)",
						        "borderColor": "rgb(54, 162, 235)",
						        "data": [
						          5,
						          68,
						          19,
						          -57,
						          -79,
						          37,
						          -24
						        ]
},
						      {
						        "type": "line",
						        "label": "Dataset 3",
						        "backgroundColor": "rgba(75, 192, 192, 0.5)",
						        "borderColor": "rgb(75, 192, 192)",
						        "fill": false,
						        "data": [
						          -35,
						          33,
						          -49,
						          2,
						          68,
						          35,
						          -16
						        ]
						      }
						    ]
						  },

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
