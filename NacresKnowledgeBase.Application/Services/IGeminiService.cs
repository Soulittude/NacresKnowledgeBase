using Pgvector;

namespace NacresKnowledgeBase.Application.Services;

public interface IGeminiService
{
    Task<Vector> GetEmbeddingAsync(string text, CancellationToken cancellationToken);
    Task<string> GetChatCompletionAsync(string question, string context, CancellationToken cancellationToken);
}