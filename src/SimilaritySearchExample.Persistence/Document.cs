namespace ResourceProjectDatabase;

public class Document
{
    public int Id { get; set; }

    public required string FileName { get; set; }
    public required string Hash { get; set; }

    public required string ContentType { get; set; }
    public required string ContainerName { get; set; }
    public required byte[] Content { get; set; }

    public List<DocumentData> Data { get; set; } = [];
    public List<DocumentVector> Vectors { get; set; } = [];
}

