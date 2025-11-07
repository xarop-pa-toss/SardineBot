using NetCord;
using SardineBot.Modules.Models;
namespace SardineBot.Database;

public static class TextInputExtensions
{
    public static Membro ToMembro(this IEnumerable<IComponent> components)
    {
        var membro = new Membro();

        foreach (var comp in components)
        {
            switch (comp)
            {
                case TextInput input:
                    switch (input.CustomId)
                    {
                        case "nome": membro.Nome = input.Value; break;
                        case "num_socio": membro.NumSocio = input.Value; break;
                        case "nif": membro.Nif = input.Value; break;
                        case "telef": membro.Telef = input.Value; break;
                        case "email": membro.Email = input.Value; break;
                        case "morada": membro.Morada = input.Value; break;
                        case "cod_postal": membro.CodPostal = input.Value; break;
                        case "localidade": membro.Localidade = input.Value; break;
                    }
                    break;

                case UserMenu userMenu when userMenu.CustomId == "discord_username":
                    membro.DiscordUsername = userMenu.SelectedValues[0];

                    break;
            }
        }

        return membro;
    }
}