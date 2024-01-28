using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Discord.Commands;
using Microsoft.Extensions.Configuration;
using RestSharp;

using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Google.Apis.Sheets.v4.Data;
using System.Reflection;
using Discord.WebSocket;
using static System.Net.WebRequestMethods;

namespace SardineBot.Commands.GoogleSheets
{
    public class GoogleSheets : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;
        private SheetsController _SheetsController;
        private SocketSlashCommand _command;

        public GoogleSheets(IConfiguration configuration)
        {
            _configuration = configuration;
            _SheetsController = new SheetsController(_configuration);
        }

        [Command("quotas")]
        [Summary("Busca estado das quotas do user ao ficheiro Google Sheets na Drive do clube")]

        public async Task<string> ExecuteAsync(SocketSlashCommand command)
        {
            _command = command;

            string realName = _SheetsController.GetRealName();

            return valueRange.Values[0][0].ToString();
        }
    }

    internal class SheetsController
    {
        private readonly IConfiguration _configuration;
        private static SheetsService _sheetsService;
        private static string _jsonPath { get; set; }


        public SheetsController(IConfiguration configuration)
        {
            _configuration = configuration;

            if (_jsonPath == null)
            {
                // Get location of currently executing assembly and combine with JSON path
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                string assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

                string projectPathToJSON = Path.Combine("Commands", "GoogleSheets", "GoogleSheetsKey.json");
                _jsonPath = Path.Combine(assemblyDirectory, projectPathToJSON);
            }

            if (_sheetsService == null) 
            {
                try
                {
                    // Create credentials from the JSON file taken from Google Service Account
                    GoogleCredential credential;

                    credential = GoogleCredential
                        .FromJson(File.ReadAllText(_jsonPath))
                        .CreateScoped(SheetsService.Scope.Spreadsheets);

                    // Create static Sheets API service
                    _sheetsService = new SheetsService(new BaseClientService.Initializer()
                    {
                        ApplicationName = "SardineBot",
                        HttpClientInitializer = credential,
                    });
                }
                catch (Exception e)
                {
                    throw new Exception(e.ToString());
                }
            }
        }
        
        internal async Task<ValueRange> GetRangeFromSheet(string range, string sheetID, string majorDimension)
        {
            // https://developers.google.com/sheets/api/samples/reading for info
            // https://developers.google.com/sheets/api/reference/rest/v4/spreadsheets.values for info on Major Dimension
            // Ranges are to be in the A1 notation. An example range is A1:D5.

            if (_configuration["GoogleSheets_ListaSociosFileID"] == null) { Console.WriteLine("FileID token is missing. Check secrets."); throw new Exception(); }

            try
            {
                var response = await _sheetsService.Spreadsheets.Values.Get(_configuration[sheetID], range).ExecuteAsync();

                // Return only if not null
                if (response.Values != null && response.Values.Count > 0)
                {
                    return response;
                }
                response.MajorDimension = "ROWS";

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao ler do Google Sheets.\n Erro: {ex.Message}");
            }
        }

        internal async Task<string> GetRealName(string range)
        {
            if (_configuration["GoogleSheets_ListaSociosFileID"] == null) { Console.WriteLine("FileID token is missing. Check secrets."); throw new Exception(); }

            try
            {
                ValueRange colHeaders = await GetRangeFromSheet(_configuration["GoogleSheets_SheetDetalhesID"], "A1:A", "ROWS");

                // ValueRange is an array of arrays. Flattening it with SelectMany turns it into a single array
                IEnumerable<object> flatList = colHeaders.Values
                    .SelectMany(row => row)
                    .Select(header => (header as string)?.ToLower());

                // IndexOf returns -1 if it can't find the value
                int discordColIndex = flatList.ToList().IndexOf("discord");

                if (discordColIndex == -1) { throw new Exception("GoogleSheets.GetRealName - Could not find specified column header"); }


            }
            catch (Exception ex) 
            { 
                Console.WriteLine("GoogleSheets.GetRealName - Couldn't read from the given range.");
                if (ex.Message != null)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }

    }
}