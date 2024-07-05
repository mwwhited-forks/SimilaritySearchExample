namespace SimilaritySearchExample.Web.Models;

public record ResourceReference
{
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
    public required string Source { get; init; }
}
