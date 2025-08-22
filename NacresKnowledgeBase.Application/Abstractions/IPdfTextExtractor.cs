namespace NacresKnowledgeBase.Application.Abstractions;

public interface IPdfTextExtractor
{
    IEnumerable<(int PageNumber, string Text)> ExtractText(Stream pdfStream);
}