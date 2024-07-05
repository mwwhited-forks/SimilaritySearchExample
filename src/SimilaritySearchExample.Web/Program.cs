using Eliassen.Common;
using ResourceProfiler.Web.Extensions;
using ResourceProfiler.Web.Handlers;
using ResourceProjectDatabase;

namespace ResourceProfiler.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //Add Application Services here
        builder.Services.AddEventQueueHandlers();
        builder.Services.AddApplicationDatabase(builder.Configuration);
        builder.Services.AddApplicationBlobContainers();

        // Add services to the container.

        builder.Services.TryAllCommonExtensions(builder.Configuration,
            externalBuilder: new()
            {
            },
            hostingBuilder: new()
            {
                DisableMailKit = true,
                DisableMessageQueueing = false,
            });

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAllCommonMiddleware();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
