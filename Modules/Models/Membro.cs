using NetCord;
namespace SardineBot.Modules.Models;

public class Membro
{
    public string Nome { get; set; } = string.Empty;
    public string Nif { get; set; } = string.Empty;
    public string NumSocio { get; set; } = string.Empty;
    public string Telef { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Morada { get; set; } = string.Empty;
    public string CodPostal { get; set; } = string.Empty;
    public string Localidade { get; set; } = string.Empty;
    public User? DiscordUsername { get; set; } = null;
}