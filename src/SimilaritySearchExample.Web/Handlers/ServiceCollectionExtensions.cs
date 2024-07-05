
using Eliassen.MessageQueueing;

namespace ResourceProfiler.Web.Handlers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventQueueHandlers(this IServiceCollection services)
    {
        services.AddTransient<IMessageQueueHandler, ResourceHandler>();
        services.AddTransient<IMessageQueueHandler, TextResourceHandler>();
        services.AddTransient<IMessageQueueHandler, IndexerHandler>();

        return services;
    }
}
