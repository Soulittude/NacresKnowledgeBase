namespace NacresKnowledgeBase.Core.Entities;

public class Document
{
    public Guid Id { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedOn { get; set; }
}