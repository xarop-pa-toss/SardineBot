using System.Data;
namespace SardineBot.Database.Models;

public class QueryResult
{
    public bool Success { get; set; }
    public DataTable? ResultTable { get; set; }
    public string? ErrorMessage { get; set; }
}