using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SnapThought.Api.Services;

/// <summary>
/// Alternate reformatting provider for the local-machine vision: calls a locally running
/// Ollama instance (<c>/api/generate</c>). Selected by setting <c>Llm:Provider=Ollama</c>.
/// If Ollama is unreachable or returns nothing usable, falls back to the original content
/// so capture never hard-fails on a down LLM.
/// </summary>
public class OllamaReformatService : IReformatService
{
    private readonly HttpClient _http;
    private readonly ILogger<OllamaReformatService> _logger;
    private readonly string _model;

    public OllamaReformatService(HttpClient http, IConfiguration config, ILogger<OllamaReformatService> logger)
    {
        _http = http;
        _logger = logger;
        _model = config["Ollama:Model"] ?? "llama3.2";
    }

    public async Task<string> ReformatAsync(string content, IEnumerable<string> tags, CancellationToken ct = default)
    {
        var prompt = ReformatPrompt.Build(content, tags);

        try
        {
            var response = await _http.PostAsJsonAsync("/api/generate", new
            {
                model = _model,
                prompt,
                stream = false
            }, ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: ct);
            var formatted = result?.Response?.Trim();
            return string.IsNullOrWhiteSpace(formatted) ? content : formatted;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama reformat failed; falling back to original content.");
            return content;
        }
    }

    private sealed record OllamaGenerateResponse(
        [property: JsonPropertyName("response")] string? Response);
}
