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

namespace SardineBot.Commands.GoogleSheets
{
    public class GoogleSheets : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;

        public GoogleSheets(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Command("quotas")]
        [Summary("Busca estado das quotas do user ao ficheiro Google Sheets na Drive do clube")]

        public async Task ExecuteAsync([Remainder][Summary("Buscar estado das quotas no ficheiro Google Sheets \"Lista Sócios\"")] string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
            {
                await ReplyAsync($"Utilização: !sardine quotas");
                return;
            }

            SheetsController sheetsControler = new SheetsController(_configuration);
            ValueRange valueRange = await sheetsControler.ReadRangeFromSheet(_configuration["GoogleSheets_SheetQuotasToken"], "A3");

            await ReplyAsync(valueRange.Values[0][0].ToString());
        }
    }

    internal class SheetsController
    {
        private readonly IConfiguration _configuration;
        private static SheetsService _sheetsService;
        private readonly string JSONPATH = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "GoogleSheets",
            "GoogleSheetsKey.json");

        internal SheetsController(IConfiguration configuration)
        {
            if (_sheetsService == null) { InitializeSheetsService(); }
            _configuration = configuration;
        }

        internal async Task<Google.Apis.Sheets.v4.Data.ValueRange> ReadRangeFromSheet(string sheetIDJsonKey, string range)
        {
            if (_configuration[sheetIDJsonKey] == null)
            {
                throw new Exception("Urban Dictionary token is missing. Check secrets.");
            }

            try
            {
                var response = await _sheetsService.Spreadsheets.Values.Get(_configuration[sheetIDJsonKey], range).ExecuteAsync();

                // Return only if not null
                if (response.Values != null && response.Values.Count > 0)
                {
                    return response;
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao ler do Google Sheets.\n Erro: {ex.Message}");
            }
            }

        private void InitializeSheetsService()
        {
            // Create credentials from the JSON file taken from Google Service Account
            GoogleCredential credential;
            using (FileStream jsonStream = new FileStream(JSONPATH, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential
                    .FromJson(jsonStream.ToString())
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            // Create static Sheets API service
            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                ApplicationName = "SardineBot",
                HttpClientInitializer = credential
            });
        }
    }
}