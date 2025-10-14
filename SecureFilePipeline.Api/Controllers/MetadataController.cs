using Microsoft.AspNetCore.Mvc;
using SecureFilePipeline.Db;
using Microsoft.EntityFrameworkCore;

namespace SecureFilePipeline.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetadataController : ControllerBase
{
    private readonly FileMetadataContext _db;

    public MetadataController(FileMetadataContext db)
    {
        _db = db;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMetadata(Guid id)
    {
        var metadata = await _db.Files.FirstOrDefaultAsync(f => f.Id == id);

        if (metadata == null)
            return NotFound(new { Message = $"Metadata with Id {id} not found" });

        return Ok(metadata);
    }
}