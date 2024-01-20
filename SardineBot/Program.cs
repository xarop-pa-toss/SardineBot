using System;
using System.Management;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SardineBot
{
    class Program
{
    private DiscordSocketClient _client;

    static async Task Main(string[] args) =>
        MainAsync(args).GetAwaiter().GetResult();


    public static async Task MainAsync(string[] args)
    {
        var configuration = new ConfigurationBuilder()
        .AddUserSecrets(Assembly.GetExecutingAssembly())
        .Build();

        var serviceProvider = new ServiceCollection()
            .AddLogging(options =>
            {
                options.ClearProviders();
                // options.AddConsole();
            })
            .AddSingleton<IConfiguration>(configuration)
            .AddScoped<IBot, Bot>()
            .BuildServiceProvider();    
    }
    public async Task RunBotAsync()
    {
        // Listen for Events
        _client = new DiscordSocketClient();
        _client.Log += LogAsync;
        _client.Ready += () => 
		{
			Console.WriteLine("The Sardine is ready to grill!!");
			return Task.CompletedTask;
		};
        _client.MessageReceived += MessageReceivedAsync;

        await _client.LoginAsync(TokenType.Bot, Secrets.BOT_TOKEN);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }
    
    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Content.ToLower() == "edgar")
        {
            await message.Channel.SendMessageAsync("é uma beca panisgas 💜");
        }
    }
}

}
