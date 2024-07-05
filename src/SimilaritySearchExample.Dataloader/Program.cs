using GreenOnion.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SimilaritySearchExample.Dataloader;

internal class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", optional: true)
            .AddCommandLine(args, new Dictionary<string, string>
            {
                ["--connection-string"] = $"{nameof(DataloaderOptions)}:{nameof(DataloaderOptions.ConnectionString)}",
                ["--action"] = $"{nameof(DataloaderOptions)}:{nameof(DataloaderOptions.Action)}",
                ["--path"] = $"{nameof(DataloaderOptions)}:{nameof(DataloaderOptions.Path)}",
            })
            .Build()
            ;

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .Configure<DataloaderOptions>(options => config.Bind(nameof(DataloaderOptions), options))
            .AddDbContext<GreenOnionContext>((sp, opt) =>
                opt.UseSqlServer(sp.GetRequiredService<IOptions<DataloaderOptions>>().Value.ConnectionString)
#if DEBUG
                .EnableSensitiveDataLogging()
#endif
                )
            .AddLogging(opt =>
                opt.AddConsole()
#if DEBUG
                    .SetMinimumLevel(LogLevel.Information)
#endif
            )
            .AddTransient<IDataLoaderFunctions, DataLoaderFunctions>()
            .AddTransient<IExpressionBuilder, ExpressionBuilder>()
            .BuildServiceProvider()
            ;

        var function = services.GetRequiredService<IDataLoaderFunctions>();

        await function.ActionAsync();
    }
}
