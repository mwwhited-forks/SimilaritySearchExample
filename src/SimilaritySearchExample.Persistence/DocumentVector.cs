using Pgvector;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceProjectDatabase;

[Table("DocumentVector")]
public class DocumentVector
{
    public int Id { get; set; }

    [Column(TypeName = "vector(768)")]
    public Vector? Embedding { get; set; }

    public List<DocumentData> Data { get; set; } = [];
}

