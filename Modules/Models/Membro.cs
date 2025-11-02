namespace SardineBot.Modules.Models;

public record Membro
{
    public required string Nome { get; init; }
    public string Nif { get; init; } = string.Empty;
    public string Tel { get; init; } = string.Empty;
    public required string Email { get; init; }
    public string Morada { get; init; } = string.Empty;
}