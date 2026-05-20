using Microsoft.AspNetCore.Mvc;
using SnapThought.Api.Models;
using SnapThought.Api.Services;

namespace SnapThought.Api.Controllers;

[ApiController]
[Route("api/snapthoughts")]
public class SnapThoughtsController : ControllerBase
{
    private readonly ISnapThoughtService _service;

    public SnapThoughtsController(ISnapThoughtService service) => _service = service;

    /// <summary>Capture a raw thought, reformat it via the local LLM, and store both versions.</summary>
    [HttpPost]
    public async Task<ActionResult<SnapThoughtResponse>> Capture(
        [FromBody] CaptureSnapThoughtRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Content is required.");

        var created = await _service.CaptureAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Retrieve a single stored thought by id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SnapThoughtResponse>> GetById(Guid id, CancellationToken ct)
    {
        var found = await _service.GetByIdAsync(id, ct);
        return found is null ? NotFound() : Ok(found);
    }

    /// <summary>List all stored thoughts, newest first.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SnapThoughtResponse>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));
}
