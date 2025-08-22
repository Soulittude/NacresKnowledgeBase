using MediatR;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using NacresKnowledgeBase.Application.Abstractions;
using NacresKnowledgeBase.Application.Services;

namespace NacresKnowledgeBase.Application.Features.Documents.Queries;

public class AskQuestionQueryHandler : IRequestHandler<AskQuestionQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly IGeminiService _geminiService; // Değiştirildi

    public AskQuestionQueryHandler(IApplicationDbContext context, IGeminiService geminiService)
    {
        _context = context;
        _geminiService = geminiService; // Değiştirildi
    }

    public async Task<string> Handle(AskQuestionQuery request, CancellationToken cancellationToken)
    {
        // --- ADIM 1: KULLANICININ SORUSUNU BİR VEKTÖRE DÖNÜŞTÜR ---

        var questionEmbedding = await _geminiService.GetEmbeddingAsync(request.Question, cancellationToken);


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
        var answer = await _geminiService.GetChatCompletionAsync(request.Question, contextText, cancellationToken);

        return answer;
    }
}