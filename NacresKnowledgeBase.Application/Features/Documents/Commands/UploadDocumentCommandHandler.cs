using Azure.AI.OpenAI;
using MediatR;
using Microsoft.Extensions.Configuration;
using NacresKnowledgeBase.Core.Entities;
using NacresKnowledgeBase.Infrastructure.Persistence;
using Pgvector; // Bu using'i ekleyin
using UglyToad.PdfPig;

namespace NacresKnowledgeBase.Application.Features.Documents.Commands;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly ApplicationDbContext _context;
    private readonly OpenAIClient _openAIClient;

    // IConfiguration'ı enjekte ederek API anahtarına erişiyoruz
    public UploadDocumentCommandHandler(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;

        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured in user secrets.");
        }

        _openAIClient = new OpenAIClient(apiKey);
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

        // --- BURASI YAPAY ZEKA ENTEGRASYONU ---
        // Her metin parçası için OpenAI API'sini çağırarak embedding oluştur
        foreach (var chunk in textChunks)
        {
            // text-embedding-3-small, OpenAI'nin en yeni ve verimli embedding modellerinden biridir.
            var options = new EmbeddingsOptions("text-embedding-3-small", new[] { chunk.Content });

            var response = await _openAIClient.GetEmbeddingsAsync(options, cancellationToken);

            // API'den gelen float[] dizisini al
            var embeddingArray = response.Value.Data[0].Embedding.ToArray();

            // Veritabanına kaydetmek için Pgvector.Vector tipine dönüştür
            chunk.Embedding = new Vector(embeddingArray);
        }
        // --- YAPAY ZEKA KISMI SONU ---

        // Dökümanı ve "akıllandırılmış" metin parçalarını veritabanına ekle
        await _context.Documents.AddAsync(document, cancellationToken);
        await _context.TextChunks.AddRangeAsync(textChunks, cancellationToken);

        // Değişiklikleri kaydet
        await _context.SaveChangesAsync(cancellationToken);

        return document.Id;
    }
}