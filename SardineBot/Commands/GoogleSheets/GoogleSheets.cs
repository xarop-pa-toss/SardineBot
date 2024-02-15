using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using System.CodeDom;
using System.Diagnostics;
using System.Reflection;
using static System.Net.WebRequestMethods;

namespace SardineBot.Commands.GoogleSheets;

public class GoogleSheets : InteractionModuleBase
{
    private readonly IConfiguration _configuration;
    private SheetsController _SheetsController;

    public GoogleSheets(IConfiguration configuration)
    {
        _configuration = configuration;
        _SheetsController = new SheetsController(_configuration);
    }

    [SlashCommand("quotas", "Busca estado das quotas do utilizador no ficheiro na Drive do clube")]
    public async Task GoogleSheetsAsync()
    {
        IDiscordInteraction command = Context.Interaction;
        IUser user = command.User;
        
        if (_configuration["GoogleSheets_ListaSociosFileID"] == null) { Console.WriteLine("FileID token is missing. Check secrets."); throw new Exception(); }


        // Sheet Detalhes has Full name on Column A, Discord names/row number on Columns H and I
        // Data starts on Row 2
        try
        {
            ValueRange discordNames = await _SheetsController.GetRangeFromSheet("Detalhes!H2:I", MajorDimensionType.ROWS, _configuration["GoogleSheets_ListaSociosFileID"]);
            int userRow = Convert.ToInt32(discordNames.Values.FirstOrDefault(row => (string)row[0] == user.Username)?.ElementAt(1) as string);

            if (userRow != null)
            {
                ValueRange realNamesDetalhesRange = await _SheetsController.GetRangeFromSheet($"Detalhes!A{userRow}", MajorDimensionType.ROWS, _configuration["GoogleSheets_ListaSociosFileID"]);
                string realName = realNamesDetalhesRange.Values[0][0].ToString();

                // Get all names in Quotas sheet -> find name that matches realName -> get entire row for that member
                ValueRange realNamesQuotasRange = await _SheetsController.GetRangeFromSheet("Quotas!A3:A", MajorDimensionType.ROWS, _configuration["GoogleSheets_ListaSociosFileID"]);

                // Array Flattening to facilitate search. Check https://codedamn.com/news/javascript/flattening-an-array-best-practices-and-examples for info
                var realNamesQuotasRange_flattened = realNamesQuotasRange.Values.SelectMany(x => x);

                int quotasRowIndex = realNamesQuotasRange_flattened.ToList().IndexOf(realName) + 3;

                //int quotasRowIndex = realNamesQuotasRange.Values
                //    .Select((value, index) => new {Value = value, Index = index})
                //    .ToList()
                //    .FirstOrDefault(x => x.Value.ToString().Equals(realName, StringComparison.OrdinalIgnoreCase))?.Index + 3 ?? -1;

                if (quotasRowIndex == -1)
                {
                    LogService.LogAsync(new LogMessage(LogSeverity.Error, "GoogleSheets.cs", "Não foi possivel encontrar o membro no documento."));
                    return;
                }


                // REPLY TO USER
                // M is used in the range to account for future added columns. It is fine since the API doesn't return trailing empty cells.
                ValueRange quotasRow = await _SheetsController.GetRangeFromSheet($"Quotas!A{quotasRowIndex}:M{quotasRowIndex}", MajorDimensionType.ROWS, _configuration["GoogleSheets_ListaSociosFileID"]);

                string dateExpiredMembership = quotasRow.Values[0][4].ToString();
                int daysUntilExpiredMembership = Convert.ToInt32(quotasRow.Values[0][6]);

                if (daysUntilExpiredMembership >= 0) {
                    await RespondAsync($"Ainda tens {daysUntilExpiredMembership} dias até perderes o estatuto de membro ({dateExpiredMembership}).");
                } else {
                    await RespondAsync($"As tuas quotas expiraram há {daysUntilExpiredMembership * -1}... Paga o que deves cabrão! ({dateExpiredMembership})");
                    return;
                }
            }
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

    private string StringFilterOutCRLF(string str)
    {
        str = str.Replace("\r", "");
        str = str.Replace("\n", "");
        return str;
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

    internal async Task<ValueRange> GetRangeFromSheet(string range, MajorDimensionType majorDimension, string spreadsheetID)
    {


        // https://developers.google.com/sheets/api/samples/reading for info
        // https://developers.google.com/sheets/api/reference/rest/v4/spreadsheets.values for info on Major Dimension
        // Ranges are to be in the A1 notation. An example range is A1:D5.

        // Variables used as parameters on all Sheets API methods must be named exactly as the parameter name. In this case, spreadsheetID and range must be named exactly as is

        if (_configuration["GoogleSheets_ListaSociosFileID"] == null) { Console.WriteLine("FileID token is missing. Check secrets."); throw new Exception(); }

        try
        {
            var response = await _sheetsService.Spreadsheets.Values.Get(spreadsheetID, range).ExecuteAsync();

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