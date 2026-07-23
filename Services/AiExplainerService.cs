using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BugBuddy.Models;

namespace BugBuddy.Services;

/// <summary>
/// OpenAI API kullanarak build hatalarını samimi ve dostça bir dille açıklar.
/// API key yoksa veya istek başarısız olursa BuiltInExplainerService'e fallback yapar.
/// </summary>
public class AiExplainerService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _provider;

    private const string SystemPrompt = """
        You are BugBuddy, a super friendly and supportive coding assistant. 
        Your job is to explain .NET/C# build errors in a warm, casual, and encouraging way.
        
        Rules:
        - Be friendly and supportive, like a helpful coworker
        - Keep explanations simple and beginner-friendly
        - Always provide a concrete solution
        - If possible, include a small code example
        - Use casual language, avoid jargon
        - NEVER make the developer feel bad about the error
        - Keep responses concise (max 3-4 sentences for explanation, max 3-4 lines for solution)
        
        Respond ONLY in valid JSON format with this structure:
        {
            "friendlyMessage": "your friendly explanation here",
            "solution": "step-by-step solution here",
            "codeExample": "optional short code example or null"
        }
        """;

    public AiExplainerService(string apiKey, string model = "gpt-4o-mini", string provider = "openai")
    {
        _apiKey = apiKey;
        _model = model;
        _provider = provider;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        if (_provider.Equals("openai", StringComparison.OrdinalIgnoreCase))
        {
            _httpClient.BaseAddress = new Uri("https://api.openai.com/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }
    }

    /// <summary>
    /// AI ile hata açıklaması üretir. Başarısız olursa fallback sözlüğe döner.
    /// </summary>
    public async Task<ErrorExplanation> ExplainAsync(BuildError error, string language = "en")
    {
        try
        {
            var isTurkish = language.Equals("tr", StringComparison.OrdinalIgnoreCase);
            var langInstruction = isTurkish 
                ? "- CRITICAL: Write all friendlyMessage and solution texts completely in TURKISH language in a warm, casual tone."
                : "- Write explanations in English.";

            var systemPromptWithLang = SystemPrompt + "\n" + langInstruction;

            var userMessage = $"""
                Error Code: {error.ErrorCode}
                File: {Path.GetFileName(error.FilePath)}
                Line: {error.Line}
                Original Message: {error.Message}
                Severity: {error.Severity}
                """;

            string? content = null;

            if (_provider.Equals("gemini", StringComparison.OrdinalIgnoreCase))
            {
                var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
                var body = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = systemPromptWithLang + "\n\n" + userMessage } }
                        }
                    }
                };

                var response = await _httpClient.PostAsJsonAsync(geminiUrl, body);
                response.EnsureSuccessStatusCode();

                var geminiResult = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                content = geminiResult?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                // Markdown json backticklerini temizle (Gemini bazen ```json ... ``` ile sarar)
                if (!string.IsNullOrWhiteSpace(content))
                {
                    content = content.Trim();
                    if (content.StartsWith("```json")) content = content[7..];
                    if (content.StartsWith("```")) content = content[3..];
                    if (content.EndsWith("```")) content = content[..^3];
                    content = content.Trim();
                }
            }
            else
            {
                var requestBody = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = systemPromptWithLang },
                        new { role = "user", content = userMessage }
                    },
                    temperature = 0.7,
                    max_tokens = 500
                };

                var response = await _httpClient.PostAsJsonAsync("v1/chat/completions", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OpenAiResponse>();
                content = result?.Choices?.FirstOrDefault()?.Message?.Content;
            }

            if (string.IsNullOrWhiteSpace(content))
                return BuiltInExplainerService.Explain(error, language);

            // JSON yanıtı parse et
            var explanation = JsonSerializer.Deserialize<AiExplanationResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (explanation is null)
                return BuiltInExplainerService.Explain(error, language);

            return new ErrorExplanation(
                explanation.FriendlyMessage ?? error.Message,
                explanation.Solution ?? "Check the documentation for this error code.",
                explanation.CodeExample
            );
        }
        catch
        {
            // API hatası durumunda fallback sözlüğe dön
            return BuiltInExplainerService.Explain(error, language);
        }
    }

    // OpenAI API yanıt modelleri
    private record OpenAiResponse(
        [property: JsonPropertyName("choices")] List<Choice>? Choices
    );

    private record Choice(
        [property: JsonPropertyName("message")] MessageContent? Message
    );

    private record MessageContent(
        [property: JsonPropertyName("content")] string? Content
    );

    private record AiExplanationResponse(
        [property: JsonPropertyName("friendlyMessage")] string? FriendlyMessage,
        [property: JsonPropertyName("solution")] string? Solution,
        [property: JsonPropertyName("codeExample")] string? CodeExample
    );

    // Gemini API yanıt modelleri
    private record GeminiResponse(
        [property: JsonPropertyName("candidates")] List<GeminiCandidate>? Candidates
    );

    private record GeminiCandidate(
        [property: JsonPropertyName("content")] GeminiContent? Content
    );

    private record GeminiContent(
        [property: JsonPropertyName("parts")] List<GeminiPart>? Parts
    );

    private record GeminiPart(
        [property: JsonPropertyName("text")] string? Text
    );
}
