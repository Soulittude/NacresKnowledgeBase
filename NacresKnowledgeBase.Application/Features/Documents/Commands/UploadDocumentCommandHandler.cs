// Artık bu kütüphanelere ihtiyacımız yok
// using Azure.AI.OpenAI;
// using Microsoft.Extensions.Configuration;

using MediatR;
using NacresKnowledgeBase.Core.Entities;
using NacresKnowledgeBase.Infrastructure.Persistence;
using Pgvector;
using UglyToad.PdfPig;

namespace NacresKnowledgeBase.Application.Features.Documents.Commands;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly ApplicationDbContext _context;
    // OpenAIClient ve IConfiguration'ı constructor'dan kaldırıyoruz

    public UploadDocumentCommandHandler(ApplicationDbContext context)
    {
        _context = context;
        // API anahtarı kontrolünü tamamen kaldırıyoruz
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
        var random = new Random();
        foreach (var chunk in textChunks)
        {
            // OpenAI'nin model boyutuna (1536) uygun, rastgele sayılardan oluşan bir vektör oluştur.
            // YANLIŞ: var dummyEmbedding = new float[1536];
            var dummyEmbedding = new float[768]; // DOĞRUSU BU

            // for döngüsü de .Length kullandığı için otomatik olarak doğru çalışacaktır.
            for (int i = 0; i < dummyEmbedding.Length; i++)
            {
                // -1 ile 1 arasında rastgele bir sayı ata
                dummyEmbedding[i] = (float)(random.NextDouble() * 2 - 1);
            }

            // Veritabanına kaydetmek için Pgvector.Vector tipine dönüştür
            chunk.Embedding = new Vector(dummyEmbedding);
        }
        // --- DEĞİŞİKLİK BURADA BİTİYOR ---

        await _context.Documents.AddAsync(document, cancellationToken);
        await _context.TextChunks.AddRangeAsync(textChunks, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return document.Id;
    }
}