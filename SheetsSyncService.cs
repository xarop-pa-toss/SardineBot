using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
namespace SardineBot;

public class SheetsSyncService
{
    private readonly SheetsService _sheetsService;
    private readonly string _sheetId;

    public SheetsSyncService(string credentialsPath, string spreadsheetId)
    {
        GoogleCredential credentials = GoogleCredential.FromFile(credentialsPath)
            .CreateScoped(SheetsService.Scope.Spreadsheets);

        _sheetsService = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credentials,
            ApplicationName = "SardineBotSheetsSync"
        });

        _sheetId = spreadsheetId;
    }

    public async Task UpdateSheetAsync(List<IList<object>> data, string range)
    {
        var valRange = new ValueRange { Values = data };

        var updateOperation = _sheetsService.Spreadsheets.Values.Update(valRange, _sheetId, range);
        updateOperation.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

        await updateOperation.ExecuteAsync();
    }
}