using Microsoft.Extensions.DependencyInjection;

namespace SardineBot
{
    public interface IBot
    {
        Task StartAsync(ServiceProvider services);
        
        Task StopAsync();
    }
}