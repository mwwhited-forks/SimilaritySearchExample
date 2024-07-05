using Microsoft.EntityFrameworkCore;

namespace SimilaritySearchExample.Persistence;

public class ResourceProfilerContext : DbContext
{
    public ResourceProfilerContext(DbContextOptions<ResourceProfilerContext> options) :
        base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
