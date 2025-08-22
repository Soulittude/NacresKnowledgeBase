using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NacresKnowledgeBase.Application.Services;
using Pgvector;

namespace NacresKnowledgeBase.Infrastructure.Services;

// Bu yardımcı sınıflar, API'den gelen JSON'ı C# nesnelerine dönüştürmek için kullanılır.
file class GeminiEmbeddingRequest(string text)
{
    public Content Content { get; set; } = new(text);
}
file class Content(string text)
{
    public Part[] Parts { get; set; } = { new(text) };
}
file class Part(string text)
{
    public string Text { get; set; } = text;
}
file class GeminiEmbeddingResponse
{
    public Embedding Embedding { get; set; } = null!;
}
file class Embedding
{
    public float[] Values { get; set; } = [];
}


public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string EmbeddingModel = "embedding-001"; // Gemini'nin embedding modeli

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not found.");
    }

    public async Task<Vector> GetEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        var requestUrl = $"v1beta/models/{EmbeddingModel}:embedContent?key={_apiKey}";
        var payload = new GeminiEmbeddingRequest(text);

        var response = await _httpClient.PostAsJsonAsync(requestUrl, payload, cancellationToken);
        response.EnsureSuccessStatusCode(); // Hata varsa exception fırlat

        var embeddingResponse = await response.Content.ReadFromJsonAsync<GeminiEmbeddingResponse>(cancellationToken: cancellationToken);

        return new Vector(embeddingResponse!.Embedding.Values);
    }

    public Task<string> GetChatCompletionAsync(string question, string context, CancellationToken cancellationToken)
    {
        // Bu kısmı bir sonraki adımda dolduracağız.
        string debugResponse = $"SUCCESS! Gemini would answer the question '{question}' based on this context:\n\n---\n{context}";
        return Task.FromResult(debugResponse);
    }
}