using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using SardineBot;
using SardineBot.Modules;

var builder = Host.CreateApplicationBuilder(args);

builder.Environment.EnvironmentName = "Development";

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: true);

var token = builder.Configuration["Discord:Token"];


#region Services
builder.Services
    .AddMemoryCache()
    .AddSingleton<GoogleSheetsSyncService>()
    .AddDiscordGateway(options =>
    {
        options.Token = token;
        options.Intents = GatewayIntents.GuildMessages
                          | GatewayIntents.DirectMessages
                          | GatewayIntents.MessageContent
                          | GatewayIntents.DirectMessageReactions
                          | GatewayIntents.GuildMessageReactions
                          | GatewayIntents.GuildEmojisAndStickers
                          | GatewayIntents.GuildMessagePolls;
    })
    // .AddGatewayHandler<SardineBot.MessageCreateHandler>()
    // .AddGatewayHandlers(typeof(Program).Assembly)
    .AddComponentInteractions<ModalInteraction, ModalInteractionContext>()
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
    .AddApplicationCommands();

#endregion

var host = builder.Build();

host.AddSlashCommand("ping", "Ping!", () => "Pong!");
// host.AddUserCommand("Username", (User user) => user.Username);
// host.AddMessageCommand("Length", (RestMessage message) => message.Content.Length.ToString());

// Add commands from modules
host.AddModules(typeof(Program).Assembly);

await host.RunAsync();