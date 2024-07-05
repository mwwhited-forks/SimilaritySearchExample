using Microsoft.EntityFrameworkCore;

namespace ResourceProjectDatabase;

public class ResourceProfilerContext : DbContext
{
    public ResourceProfilerContext(DbContextOptions<ResourceProfilerContext> options) :
        base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentVector> Vectors { get; set; }
    public DbSet<MessageQueue> MessageQueue { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("vector");

        // https://github.com/pgvector/pgvector
        // https://github.com/pgvector/pgvector-dotnet

        modelBuilder.Entity<DocumentVector>()
            .HasIndex(i => i.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");
    }
}
