namespace SardineBot.Modules.Models;

public class Membro
{
    public string Nome { get; set; }
    public string Nif { get; set; } = string.Empty;
    public string NumSocio { get; set; }
    public string Telef { get; set; } = string.Empty;
    public string Email { get; set; }
    public string Morada { get; set; } = string.Empty;
    public string CodPostal { get; set; } = string.Empty;
    public string Localidade { get; set; } = string.Empty;
}