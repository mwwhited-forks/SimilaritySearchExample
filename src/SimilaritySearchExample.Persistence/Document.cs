namespace SimilaritySearchExample.Persistence;

public class Document
{
    public int Id { get; set; }

    public required string FileName { get; set; }
    public required string Hash { get; set; }

    public required string ContentType { get; set; }
    public required string ContainerName { get; set; }
}

