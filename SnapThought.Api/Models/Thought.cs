namespace SnapThought.Api.Models;

/// <summary>
/// A captured thought. Holds both the raw <see cref="OriginalContent"/> as entered
/// and the LLM-produced <see cref="FormattedContent"/>, so the two can be toggled.
/// </summary>
public class Thought
{
    public Guid Id { get; set; }

    public string OriginalContent { get; set; } = string.Empty;

    public string? FormattedContent { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
