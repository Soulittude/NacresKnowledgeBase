using Pgvector;
namespace NacresKnowledgeBase.Core.Entities;

using System.ComponentModel.DataAnnotations.Schema;

public class TextChunk
{
    public Guid Id { get; set; }

    // Hangi dökümana ait olduğunu belirtmek için
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    // Parçanın metin içeriği
    public string Content { get; set; } = string.Empty;

    // Kaçıncı sayfadan alındığı gibi ek bilgi için
    public int PageNumber { get; set; }

    // Bu kolonu ekliyoruz. "vector(1536)" PostgreSQL'e bunun bir vektör
    // olduğunu ve OpenAI'nin text-embedding-ada-002 modelinin ürettiği
    // 1536 boyutta olacağını söyler.
    [Column(TypeName = "vector(1536)")]
    public Vector? Embedding { get; set; }
}