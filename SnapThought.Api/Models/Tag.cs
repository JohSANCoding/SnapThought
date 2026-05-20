namespace SnapThought.Api.Models;

/// <summary>
/// A label attached to one or more <see cref="Thought"/>s. Tag names are unique.
/// </summary>
public class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Thought> Thoughts { get; set; } = new List<Thought>();
}
