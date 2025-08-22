using MediatR;
using Microsoft.EntityFrameworkCore;
using NacresKnowledgeBase.Infrastructure.Persistence;
using Pgvector;
using Pgvector.EntityFrameworkCore; // Vektör fonksiyonları için bu gerekli

namespace NacresKnowledgeBase.Application.Features.Documents.Queries;

public class AskQuestionQueryHandler : IRequestHandler<AskQuestionQuery, string>
{
    private readonly ApplicationDbContext _context;
    // Daha sonra buraya Gemini/OpenAI istemcisini ekleyeceğiz

    public AskQuestionQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(AskQuestionQuery request, CancellationToken cancellationToken)
    {
        // --- ADIM 1: KULLANICININ SORUSUNU BİR VEKTÖRE DÖNÜŞTÜR ---

        // ŞİMDİLİK SAHTE BİR VEKTÖR OLUŞTURUYORUZ
        var random = new Random();
        var embeddingArray = new float[768]; // Gemini için 768 boyutlu
        for (int i = 0; i < 768; i++)
        {
            embeddingArray[i] = (float)(random.NextDouble() * 2 - 1);
        }
        var questionEmbedding = new Vector(embeddingArray);
        // GERÇEKTE BURADA GEMINI API'Sİ ÇAĞIRILACAK


        // --- ADIM 2: VERİTABANINDA ANLAMSAL OLARAK EN YAKIN METİN PARÇALARINI BUL ---

        // Pgvector.EntityFrameworkCore kütüphanesi bize L2Distance gibi fonksiyonlar sağlar.
        // Bu fonksiyon, iki vektör arasındaki "mesafeyi" (farklılığı) hesaplar.
        // Mesafesi en küçük olanlar, anlamsal olarak en yakın olanlardır.
        var relevantChunks = await _context.TextChunks
            .OrderBy(c => c.Embedding!.L2Distance(questionEmbedding)) // En yakınları bul
            .Take(3) // En alakalı ilk 3 sonucu al
            .ToListAsync(cancellationToken);

        if (!relevantChunks.Any())
        {
            return "Üzgünüm, bu soruya cevap verebilecek bir bilgi bulamadım.";
        }


        // --- ADIM 3: EN ALAKALI PARÇALARI VE SORUYU BİRLEŞTİRİP YAPAY ZEKAYA GÖNDER ---

        var contextText = string.Join("\n---\n", relevantChunks.Select(c => c.Content));

        // ŞİMDİLİK YAPAY ZEKAYA GÖNDERMEK YERİNE, BULDUĞUMUZ METİNLERİ GÖSTERELİM
        // Bu, sistemimizin doğru parçaları bulup bulmadığını test etmemizi sağlar.

        string debugResponse = $"DEBUG: Sorunuza en yakın bulunan metin parçaları şunlardır:\n\n{contextText}";

        // GERÇEKTE BURADA GEMINI CHAT COMPLETION API'Sİ ÇAĞIRILACAK VE
        // debugResponse YERİNE ONDAN GELEN CEVAP DÖNDÜRÜLECEK

        return debugResponse;
    }
}