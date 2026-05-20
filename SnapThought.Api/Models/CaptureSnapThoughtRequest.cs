namespace SnapThought.Api.Models;

/// <summary>
/// Inbound payload for capturing a new thought. Tags are optional.
/// </summary>
public record CaptureSnapThoughtRequest(string Content, string[]? Tags);
