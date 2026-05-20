namespace SnapThought.Api.Services;

/// <summary>
/// Builds the provider-agnostic prompt used to reformat a raw thought. Shared by every
/// <see cref="IReformatService"/> implementation so prompt wording stays consistent.
/// </summary>
internal static class ReformatPrompt
{
    public static string Build(string content, IEnumerable<string> tags)
    {
        var tagList = tags?.ToArray() ?? Array.Empty<string>();
        var tagHint = tagList.Length > 0
            ? $"The user tagged this thought with: {string.Join(", ", tagList)}. Let these tags guide tone and structure."
            : "No tags were provided.";

        return $"""
            You are a note-formatting assistant. Reformat the raw thought below into clean, well-structured text.
            Rules:
            - Preserve the original meaning exactly. Do NOT add new ideas, facts, or commentary.
            - Only fix grammar, structure, and clarity.
            - {tagHint}
            - Respond with ONLY the reformatted text, no preamble.

            Raw thought:
            {content}
            """;
    }
}
