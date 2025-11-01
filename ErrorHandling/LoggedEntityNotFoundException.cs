using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using NetCord;
namespace SardineBot.ErrorHandling;

public class LoggedEntityNotFoundException : EntityNotFoundException
{
    public LoggedEntityNotFoundException(
        string? message = null,
        ILogger? logger = null,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
        : base(message)
    {
        logger?.LogError("EntityNotFoundException in {Caller} ({File}:{Line}): {Message}",
            caller, Path.GetFileName(file), line, message ?? "No message provided");
    }

}
