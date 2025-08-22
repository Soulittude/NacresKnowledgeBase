// Artık bu kütüphanelere ihtiyacımız yok
// using Azure.AI.OpenAI;
// using Microsoft.Extensions.Configuration;

using MediatR;
using NacresKnowledgeBase.Core.Entities;
using Pgvector;
using NacresKnowledgeBase.Application.Abstractions;
using NacresKnowledgeBase.Application.Services;

namespace NacresKnowledgeBase.Application.Features.Documents.Commands;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IGeminiService _geminiService;
    private readonly IPdfTextExtractor _pdfTextExtractor;

    public UploadDocumentCommandHandler(IApplicationDbContext context, IGeminiService geminiService, IPdfTextExtractor pdfTextExtractor)
    {
        _context = context;
        _geminiService = geminiService;
        _pdfTextExtractor = pdfTextExtractor; // Eklendi
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

        // PDF okuma kodunun tamamını silip yerine bunu yazıyoruz
        var textChunks = new List<TextChunk>();
        using (var stream = request.File.OpenReadStream())
        {
            foreach (var (pageNumber, text) in _pdfTextExtractor.ExtractText(stream))
            {
                textChunks.Add(new TextChunk
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    Content = text,
                    PageNumber = pageNumber
                });
            }
        }

        // --- DEĞİŞİKLİK BURADA BAŞLIYOR ---
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