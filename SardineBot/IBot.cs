using Microsoft.Extensions.DependencyInjection;

namespace SardineBot
{
    public interface IBot
    {
        Task StartAsync(IServiceProvider services);
        
        Task StopAsync();
    }
}