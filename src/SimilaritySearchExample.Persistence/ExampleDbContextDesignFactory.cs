using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ResourceProjectDatabase;

public class ExampleDbContextDesignFactory : IDesignTimeDbContextFactory<ResourceProfilerContext>
{
    public ResourceProfilerContext CreateDbContext(string[] args)
    {
        var connectionString = "Host=127.0.0.1;Database=my_db;Username=admin;Password=admin";
        Console.WriteLine($"Connection String: \"{connectionString}\"");

        var optionsBuilder = new DbContextOptionsBuilder<ResourceProfilerContext>();
        optionsBuilder.UseNpgsql(connectionString, o => o.UseVector());

        return new ResourceProfilerContext(optionsBuilder.Options);
    }
}
