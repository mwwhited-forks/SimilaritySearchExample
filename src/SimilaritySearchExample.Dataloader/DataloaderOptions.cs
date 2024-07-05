namespace SimilaritySearchExample.Dataloader;

public record DataloaderOptions
{
    public required string ConnectionString { get; init; }
    public required DataloaderActions Action { get; init; } = DataloaderActions.Export;
    public required string Path { get; init; }
}
