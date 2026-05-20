using SnapThought.Api.Models;

namespace SnapThought.Api.Services;

/// <summary>
/// Orchestrates the capture pipeline (persist raw → reformat → persist formatted)
/// and read access for stored thoughts.
/// </summary>
public interface ISnapThoughtService
{
    Task<SnapThoughtResponse> CaptureAsync(CaptureSnapThoughtRequest request, CancellationToken ct = default);

    Task<SnapThoughtResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<SnapThoughtResponse>> GetAllAsync(CancellationToken ct = default);
}
