using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SnapThought.Api.Services;

/// <summary>
/// Default reformatting provider. Calls Google's Gemini
/// (<c>generativelanguage.googleapis.com</c>) over its REST API. The API key is read from
/// configuration (env var <c>Gemini__ApiKey</c> or <c>dotnet user-secrets</c>) and never
/// committed. If the key is missing or the call fails, falls back to the original content
/// so capture never hard-fails.
/// </summary>
public class GeminiReformatService : IReformatService
{
    private readonly HttpClient _http;
    private readonly ILogger<GeminiReformatService> _logger;
    private readonly string _model;
    private readonly string? _apiKey;

    public GeminiReformatService(HttpClient http, IConfiguration config, ILogger<GeminiReformatService> logger)
    {
        _http = http;
        _logger = logger;
        _model = config["Gemini:Model"] ?? "gemini-2.0-flash";
        _apiKey = config["Gemini:ApiKey"];
    }

    public async Task<string> ReformatAsync(string content, IEnumerable<string> tags, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini:ApiKey is not configured; returning original content.");
            return content;
        }

        var prompt = ReformatPrompt.Build(content, tags);

        try
        {
            var response = await _http.PostAsJsonAsync(
                $"v1beta/models/{_model}:generateContent?key={_apiKey}",
                new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                }, ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: ct);
            var formatted = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text?.Trim();
            return string.IsNullOrWhiteSpace(formatted) ? content : formatted;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini reformat failed; falling back to original content.");
            return content;
        }
    }

    private sealed record GeminiResponse(
        [property: JsonPropertyName("candidates")] GeminiCandidate[]? Candidates);

    private sealed record GeminiCandidate(
        [property: JsonPropertyName("content")] GeminiContent? Content);

    private sealed record GeminiContent(
        [property: JsonPropertyName("parts")] GeminiPart[]? Parts);

    private sealed record GeminiPart(
        [property: JsonPropertyName("text")] string? Text);
}
