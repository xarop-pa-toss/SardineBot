using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

class Program
{
    #region Initialize Discord Bot
    private DiscordSocketClient _client;

    static async Task Main(string[] args)
    {
        var program = new Program();
        await program.RunBotAsync();
    }

    public async Task RunBotAsync()
    {
        _client = new DiscordSocketClient();
        _client.Log += LogAsync;

        await _client.LoginAsync(TokenType.Bot, Secrets.BOT_TOKEN);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }
    #endregion
}
