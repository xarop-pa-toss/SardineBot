using System.Data;
namespace SardineBot.DatabaseHandlers;

public class QueryResult
{
    public bool Success { get; set; }
    public DataTable? ResultTable { get; set; }
    public string? ErrorMessage { get; set; }
}