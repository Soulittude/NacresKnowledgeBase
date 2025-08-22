using System.IO;
using System.Collections.Generic;
using NacresKnowledgeBase.Application.Abstractions;
using UglyToad.PdfPig;

namespace NacresKnowledgeBase.Infrastructure.Services;

public class PdfTextExtractor : IPdfTextExtractor
{
    public IEnumerable<(int PageNumber, string Text)> ExtractText(Stream pdfStream)
    {
        using var pdfDocument = PdfDocument.Open(pdfStream);
        foreach (var page in pdfDocument.GetPages())
        {
            if (!string.IsNullOrWhiteSpace(page.Text))
            {
                yield return (page.Number, page.Text);
            }
        }
    }
}