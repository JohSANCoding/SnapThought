namespace SnapThought.Api.Models;

/// <summary>
/// Outbound representation of a stored thought, exposing both the raw and
/// reformatted text alongside its tags.
/// </summary>
public record SnapThoughtResponse(
    Guid Id,
    string Original,
    string? Formatted,
    string[] Tags,
    DateTime CreatedAt);
