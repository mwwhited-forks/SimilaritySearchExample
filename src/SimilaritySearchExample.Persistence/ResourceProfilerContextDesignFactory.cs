using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SimilaritySearchExample.Persistence;

public class ResourceProfilerContextDesignFactory : IDesignTimeDbContextFactory<ResourceProfilerContext>
{
    public ResourceProfilerContext CreateDbContext(string[] args)
    {
        var connectionString = "Server=127.0.0.1;Database=ResourceProfilerDb;User ID=sa;Password=S1m1l@1tyS3@rch;TrustServerCertificate=True;";
        Console.WriteLine($"Connection String: \"{connectionString}\"");

        var optionsBuilder = new DbContextOptionsBuilder<ResourceProfilerContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ResourceProfilerContext(optionsBuilder.Options);
    }
}
