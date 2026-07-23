using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BugBuddy.Models;

namespace BugBuddy.Services;

/// <summary>
/// İnteraktif AI sohbet oturumunu yönetir.
/// Geçmiş mesajları hafızada tutarak bağlamı (context) korur.
/// </summary>
public class AiChatService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _model;
    private readonly string _provider;
    private readonly List<ChatMessage> _messages = new();

    private const string SystemPrompt = """
        You are BugBuddy, a super friendly, casual, and encouraging C#/.NET AI pair programmer.
        Your job is to chat with developers, answer questions about C#, .NET, build errors, clean code, and design patterns.
        
        Rules:
        - Be warm, friendly, supportive, and enthusiastic.
        - Keep explanations clear, concise, and easy to understand.
        - Include short C# code examples when helpful.
        - If the user asks in Turkish or the language setting is 'tr', respond in friendly, helpful TURKISH.
        """;

    public AiChatService(string? apiKey, string model = "gpt-4o-mini", string provider = "openai")
    {
        _apiKey = apiKey;
        _model = model;
        _provider = provider;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        if (_provider.Equals("openai", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(_apiKey))
        {
            _httpClient.BaseAddress = new Uri("https://api.openai.com/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        // System prompt ekle
        _messages.Add(new ChatMessage("system", SystemPrompt));
    }

    /// <summary>
    /// Kullanıcı mesajını gönderir ve yapay zekadan yanıt döner.
    /// </summary>
    public async Task<string> SendMessageAsync(string userText, string language = "en")
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            var isTurkish = language.Equals("tr", StringComparison.OrdinalIgnoreCase);
            if (isTurkish)
            {
                return "Sohbet özelliğini kullanabilmek için AI API anahtarınızı tanımlamanız gerekmektedir.\n\n" +
                       "👉 Tanımlamak için: [cyan]bugbuddy config --provider gemini --api-key API_KEYINIZ[/]";
            }
            return "To use the interactive AI chat feature, please configure your AI API key.\n\n" +
                   "👉 Usage: [cyan]bugbuddy config --provider gemini --api-key YOUR_API_KEY[/]";
        }

        var isTr = language.Equals("tr", StringComparison.OrdinalIgnoreCase);
        var langPrompt = isTr ? "\n[Note: Respond in TURKISH]" : "";

        // Kullanıcı mesajını ekle
        _messages.Add(new ChatMessage("user", userText + langPrompt));

        try
        {
            string? reply = null;

            if (_provider.Equals("gemini", StringComparison.OrdinalIgnoreCase))
            {
                var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
                
                var geminiContents = _messages.Select(m => new
                {
                    role = m.Role is "assistant" or "model" ? "model" : "user",
                    parts = new[] { new { text = m.Content } }
                }).ToArray();

                var body = new { contents = geminiContents };

                var response = await _httpClient.PostAsJsonAsync(geminiUrl, body);
                response.EnsureSuccessStatusCode();

                var geminiResult = await response.Content.ReadFromJsonAsync<GeminiChatResponse>();
                reply = geminiResult?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            }
            else
            {
                var requestBody = new
                {
                    model = _model,
                    messages = _messages.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
                    temperature = 0.7,
                    max_tokens = 1000
                };

                var response = await _httpClient.PostAsJsonAsync("v1/chat/completions", requestBody);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>();
                reply = result?.Choices?.FirstOrDefault()?.Message?.Content;
            }

            if (string.IsNullOrWhiteSpace(reply))
            {
                return isTr 
                    ? "Üzgünüm, boş bir yanıt aldım. Lütfen tekrar deneyin." 
                    : "Sorry, I received an empty response. Please try again.";
            }

            // Asistan yanıtını geçmişe ekle
            _messages.Add(new ChatMessage("assistant", reply));

            return reply;
        }
        catch (Exception ex)
        {
            return isTr 
                ? $"Bir hata oluştu: {ex.Message}" 
                : $"An error occurred: {ex.Message}";
        }
    }

    private record ChatMessage(string Role, string Content);

    private record OpenAiChatResponse(
        [property: JsonPropertyName("choices")] List<Choice>? Choices
    );

    private record Choice(
        [property: JsonPropertyName("message")] MessageContent? Message
    );

    private record MessageContent(
        [property: JsonPropertyName("content")] string? Content
    );

    private record GeminiChatResponse(
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
