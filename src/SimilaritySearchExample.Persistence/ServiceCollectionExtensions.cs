using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ResourceProjectDatabase;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ResourceProfilerContext>(options => options.UseNpgsql(
                configuration.GetConnectionString("npgsql"),
                opt => opt.UseVector()
                ), 
        contextLifetime: ServiceLifetime.Transient,
        optionsLifetime: ServiceLifetime.Singleton
        );

        return services;
    }
}
