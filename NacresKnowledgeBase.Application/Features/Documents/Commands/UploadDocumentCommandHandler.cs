// Artık bu kütüphanelere ihtiyacımız yok
// using Azure.AI.OpenAI;
// using Microsoft.Extensions.Configuration;

using MediatR;
using NacresKnowledgeBase.Core.Entities;
using NacresKnowledgeBase.Infrastructure.Persistence;
using Pgvector;
using UglyToad.PdfPig;
using NacresKnowledgeBase.Application.Services;

namespace NacresKnowledgeBase.Application.Features.Documents.Commands;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly ApplicationDbContext _context;
    private readonly IGeminiService _geminiService;

    public UploadDocumentCommandHandler(ApplicationDbContext context, IGeminiService geminiService) // Değiştirildi
    {
        _context = context;
        _geminiService = geminiService; // Değiştirildi
    }

    public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            UploadedOn = DateTime.UtcNow
        };

        var textChunks = new List<TextChunk>();
        using (var stream = request.File.OpenReadStream())
        using (var pdfDocument = PdfDocument.Open(stream))
        {
            foreach (var page in pdfDocument.GetPages())
            {
                var text = page.Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    textChunks.Add(new TextChunk
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = document.Id,
                        Content = text,
                        PageNumber = page.Number
                    });
                }
            }
        }

        // --- DEĞİŞİKLİK BURADA BAŞLIYOR ---
        // Gerçek API çağrısı yerine sahte (dummy) embedding oluşturuyoruz
        foreach (var chunk in textChunks)
        {
            chunk.Embedding = await _geminiService.GetEmbeddingAsync(chunk.Content, cancellationToken);
        }
        // --- DEĞİŞİKLİK BURADA BİTİYOR ---

        await _context.Documents.AddAsync(document, cancellationToken);
        await _context.TextChunks.AddRangeAsync(textChunks, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return document.Id;
    }
}