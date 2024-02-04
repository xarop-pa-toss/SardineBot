using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SardineBot;

namespace DiscordBot;

internal class Program
{
    private static IConfiguration? _configuration;
    private static IServiceProvider _services;

    private static readonly DiscordSocketConfig _socketConfig = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
        AlwaysDownloadUsers = true
    };


    private static void Main(string[] args) =>
        MainAsync(args).GetAwaiter().GetResult();

    private static async Task MainAsync(string[] args)
    {
        _configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        _services = new ServiceCollection()
            .AddSingleton<IConfiguration>(_configuration)
            .AddSingleton(_socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .AddScoped<IBot, Bot>()
            .BuildServiceProvider();

        try
        {
            IBot bot = _services.GetRequiredService<IBot>();

            await bot.StartAsync(_services);

            Console.WriteLine("SardineBot is salted and ready to grill!!");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            Environment.Exit(-1);
        }
    }
}