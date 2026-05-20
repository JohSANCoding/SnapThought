using Microsoft.EntityFrameworkCore;
using SnapThought.Api.Data;
using SnapThought.Api.Models;

namespace SnapThought.Api.Services;

public class SnapThoughtService : ISnapThoughtService
{
    private readonly AppDbContext _db;
    private readonly IReformatService _reformat;

    public SnapThoughtService(AppDbContext db, IReformatService reformat)
    {
        _db = db;
        _reformat = reformat;
    }

    public async Task<SnapThoughtResponse> CaptureAsync(CaptureSnapThoughtRequest request, CancellationToken ct = default)
    {
        var tags = await ResolveTagsAsync(request.Tags, ct);

        var thought = new Thought
        {
            Id = Guid.NewGuid(),
            OriginalContent = request.Content,
            CreatedAt = DateTime.UtcNow,
            Tags = tags
        };

        // Persist the raw thought first so capture is durable even if reformatting fails.
        _db.Thoughts.Add(thought);
        await _db.SaveChangesAsync(ct);

        thought.FormattedContent = await _reformat.ReformatAsync(
            request.Content,
            tags.Select(t => t.Name),
            ct);
        await _db.SaveChangesAsync(ct);

        return Map(thought);
    }

    public async Task<SnapThoughtResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var thought = await _db.Thoughts
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        return thought is null ? null : Map(thought);
    }

    public async Task<IReadOnlyList<SnapThoughtResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var thoughts = await _db.Thoughts
            .Include(t => t.Tags)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        return thoughts.Select(Map).ToList();
    }

    /// <summary>
    /// Maps incoming tag names to existing <see cref="Tag"/> rows where possible,
    /// creating new ones for names not yet seen.
    /// </summary>
    private async Task<List<Tag>> ResolveTagsAsync(string[]? names, CancellationToken ct)
    {
        if (names is null || names.Length == 0)
            return new List<Tag>();

        var normalized = names
            .Select(n => n.Trim())
            .Where(n => n.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
            return new List<Tag>();

        var existing = await _db.Tags
            .Where(t => normalized.Contains(t.Name))
            .ToListAsync(ct);

        var result = new List<Tag>(existing);
        foreach (var name in normalized)
        {
            if (!existing.Any(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)))
                result.Add(new Tag { Name = name });
        }

        return result;
    }

    private static SnapThoughtResponse Map(Thought t) => new(
        t.Id,
        t.OriginalContent,
        t.FormattedContent,
        t.Tags.Select(tag => tag.Name).ToArray(),
        t.CreatedAt);
}
