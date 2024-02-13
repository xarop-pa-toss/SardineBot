using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using System.CodeDom;
using System.Reflection;
using static System.Net.WebRequestMethods;

namespace SardineBot.Commands.GoogleSheets
{
    public class GoogleSheets : InteractionModuleBase
    {
        private readonly IConfiguration _configuration;
        private SheetsController _SheetsController;

        public GoogleSheets(IConfiguration configuration)
        {
            _configuration = configuration;
            _SheetsController = new SheetsController(_configuration);
        }

        [SlashCommand("quotas", "Busca estado das quotas do user ao ficheiro Google Sheets na Drive do clube")]
        public async Task<string> GoogleSheetsAsync()
        {
            IDiscordInteraction command = Context.Interaction;
            IUser user = command.User;
            
            // Sheet Detalhes has Full name on Column A, Discord names/row number on Columns H and I
            // Data starts on Row 2
            if (_configuration["GoogleSheets_ListaSociosFileID"] == null) { Console.WriteLine("FileID token is missing. Check secrets."); throw new Exception(); }

            try
            {
                ValueRange discordNames = await _SheetsController.GetRangeFromSheet("Detalhes!H2:I", MajorDimensionType.ROWS, _configuration["GoogleSheets_ListaSociosFileID"]);
                int userRow = Convert.ToInt32(discordNames.Values.FirstOrDefault(row => (string)row[0] == user.Username)?.ElementAt(1) as string);

                if (userRow != null)
                {
                    ValueRange realNameValueRange = await _SheetsController.GetRangeFromSheet($"Detalhes!A{userRow}", MajorDimensionType.ROWS, _configuration["GoogleSheets_ListaSociosFileID"]);
                    return realNameValueRange.Values[0][0].ToString();
                }

                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("GoogleSheets.GetRealName - Couldn't read from the given range.");
                if (ex.Message != null)
                {
                    Console.WriteLine(ex.Message);
                }
                return "";
            }
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
                        .FromJson(System.IO.File.ReadAllText(_jsonPath))
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

        internal async Task<ValueRange> GetRangeFromSheet(string range, MajorDimensionType majorDimension, string fileID)
        {
            // https://developers.google.com/sheets/api/samples/reading for info
            // https://developers.google.com/sheets/api/reference/rest/v4/spreadsheets.values for info on Major Dimension
            // Ranges are to be in the A1 notation. An example range is A1:D5.

            if (_configuration["GoogleSheets_ListaSociosFileID"] == null) { Console.WriteLine("FileID token is missing. Check secrets."); throw new Exception(); }

            try
            {
                var response = await _sheetsService.Spreadsheets.Values.Get(_configuration[fileID], range).ExecuteAsync();

                // Return only if not null
                if (response.Values != null && response.Values.Count > 0)
                {
                    return response;
                }
                response.MajorDimension = majorDimension.ToString();

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao ler do Google Sheets.\n Erro: {ex.Message}");
            }
        }
    }

    public enum MajorDimensionType
    {
        ROWS,
        COLS
    }
}