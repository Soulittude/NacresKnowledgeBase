using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NacresKnowledgeBase.Application.Services;
using System.Text.Json.Serialization;
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

file class GeminiChatRequest
{
    [JsonPropertyName("contents")]
    public ChatContent[] Contents { get; set; } = [];
}
file class ChatContent
{
    [JsonPropertyName("parts")]
    public ChatPart[] Parts { get; set; } = [];
}
file class ChatPart
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
file class GeminiChatResponse
{
    [JsonPropertyName("candidates")]
    public Candidate[] Candidates { get; set; } = [];
}
file class Candidate
{
    [JsonPropertyName("content")]
    public ChatContent? Content { get; set; }
}


public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string EmbeddingModel = "embedding-001"; // Gemini'nin embedding modeli
    private const string ChatModel = "gemini-1.5-flash-latest"; // Daha hızlı ve verimli bir model

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

    public async Task<string> GetChatCompletionAsync(string question, string context, CancellationToken cancellationToken)
    {
        var requestUrl = $"v1beta/models/{ChatModel}:generateContent?key={_apiKey}";

        // Gemini'ye soruyu ve bulduğumuz bağlamı net bir şekilde iletiyoruz
        var prompt = $"Based on the following context, please answer the user's question.\n\nContext:\n---\n{context}\n---\n\nQuestion: {question}";

        var payload = new GeminiChatRequest
        {
            Contents = new[]
            {
                new ChatContent
                {
                    Parts = new[] { new ChatPart { Text = prompt } }
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(requestUrl, payload, cancellationToken);

        // Hata ayıklama için: Eğer API hata verirse, içeriğini görelim
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return $"Error from Gemini API: {response.StatusCode} - {errorContent}";
        }

        var chatResponse = await response.Content.ReadFromJsonAsync<GeminiChatResponse>(cancellationToken: cancellationToken);

        // API'den gelen cevabın içindeki metni alıp döndürüyoruz
        var answer = chatResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

        return answer ?? "Sorry, I couldn't generate an answer based on the provided information.";
    }
}