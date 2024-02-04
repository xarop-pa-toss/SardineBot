using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;


namespace SardineBot.Commands;

public class SlashCommandHandler : InteractionModuleBase<SocketInteractionContext>
{
    public InteractionService Commands { get; set; }
    private InteractionHandler _handler;

    public SlashCommandHandler(InteractionHandler handler)
    {
        _handler = handler;
    }


}
