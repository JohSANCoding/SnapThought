namespace SnapThought.Api.Services;

/// <summary>
/// Reformats raw thought text into clean, structured prose using a local LLM.
/// </summary>
public interface IReformatService
{
    Task<string> ReformatAsync(string content, IEnumerable<string> tags, CancellationToken ct = default);
}
