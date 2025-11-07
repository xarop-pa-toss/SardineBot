using System.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using SardineBot.Database;
namespace SardineBot;

public class GoogleSheetsSyncService
{
    private readonly SheetsService _sheetsService;
    private readonly string _spreadsheetId;

    public GoogleSheetsSyncService(IConfiguration config)
    {
        var credentials = GoogleCredential
            .FromFile(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Config/sardinebot-b07e09b10142.json"))
            .CreateScoped(SheetsService.Scope.Spreadsheets);

        _spreadsheetId = config["GoogleSheets:SpreadsheetId"];

        _sheetsService = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credentials,
            ApplicationName = "SardineBotSheetsSync"
        });
    }

    public async Task SyncSheetWithDbAsync(SheetnameEnum sheetName)
    {
        var firstCell = string.Empty;
        switch (sheetName)
        {
            case SheetnameEnum.Detalhes:
                firstCell = "A2";
                break;
            case SheetnameEnum.Quotas:
                firstCell = "A3";
                break;
        }

        // Delete old data before updating
        await _sheetsService.Spreadsheets.Values.Clear(
            new ClearValuesRequest(),
            _spreadsheetId,
            $"{sheetName}!{firstCell}:Z"
        ).ExecuteAsync();

        var range = $"{sheetName}!{firstCell}";
        var body = new ValueRange
        {
            Values = GetDataFromDbToSheetValuesAsync(sheetName).Result
        };

        var request = _sheetsService.Spreadsheets.Values.Update(body, _spreadsheetId, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
        await request.ExecuteAsync();
    }

    private async Task<IList<IList<object>>> GetDataFromDbToSheetValuesAsync(SheetnameEnum sheetName)
    {
        var query = string.Empty;
        switch (sheetName)
        {
            case SheetnameEnum.Detalhes:
                query = """
                        SELECT nome, nif, morada, cod_postal, localidade, telef, email, discord_username, num_socio
                        FROM membros
                        """;
                break;
            case SheetnameEnum.Quotas:
                query = """
                        SELECT nome, inscricao_inicio, inscricao_fim, email, num_socio
                        FROM membros
                        """;
                break;
        }

        var result = await new QueryRunner().QueryAsync(
            query
        );

        var values = new List<IList<object>>();
        foreach (DataRow row in result.ResultTable.Rows)
        {
            values.Add(row.ItemArray.Cast<object>().ToList());
        }

        return values;
    }
}