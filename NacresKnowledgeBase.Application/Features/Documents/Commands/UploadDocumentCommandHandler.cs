using MediatR;
using NacresKnowledgeBase.Core.Entities;
using NacresKnowledgeBase.Infrastructure.Persistence;
using UglyToad.PdfPig; // PdfPig kütüphanesini ekliyoruz

namespace NacresKnowledgeBase.Application.Features.Documents.Commands;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly ApplicationDbContext _context;

    public UploadDocumentCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        // 1. Döküman metadatasını oluştur (Bu kısım aynı)
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            UploadedOn = DateTime.UtcNow
        };

        // 2. Yüklenen dosyanın içeriğini oku
        var textChunks = new List<TextChunk>();

        // request.File.OpenReadStream() ile dosyanın içeriğine ulaşıyoruz
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
                        DocumentId = document.Id, // Oluşturulan dökümanla ilişkilendir
                        Content = text,
                        PageNumber = page.Number
                    });
                }
            }
        }

        // 3. Dökümanı ve okunan metin parçalarını veritabanına ekle
        await _context.Documents.AddAsync(document, cancellationToken);
        await _context.TextChunks.AddRangeAsync(textChunks, cancellationToken);

        // 4. Değişiklikleri kaydet
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Dökümanın ID'sini döndür
        return document.Id;
    }
}