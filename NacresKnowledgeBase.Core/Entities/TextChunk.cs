namespace NacresKnowledgeBase.Core.Entities;

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

    // Daha sonra buraya bu metnin anlamsal vektörünü de ekleyeceğiz
}