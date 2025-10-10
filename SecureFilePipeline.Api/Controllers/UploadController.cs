using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SecureFilePipeline.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly ILogger<UploadController> _logger;
    private readonly string _uploadPath;
    private const long MaxFileSize = 100 * 1024 * 1024; // 100Mb
    private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".doc", ".txt", ".png", ".jpg", ".jpeg" };

    public UploadController(ILogger<UploadController> logger, IConfiguration config)
    {
        _logger = logger;
        _uploadPath = config["FileStorage:UploadPath"] 
                        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileUploader", "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    [HttpPost]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadFile([Required] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file uploaded" });
            }

            if (file.Length > MaxFileSize)
            {
                return BadRequest(new { error = $"Filesize exceeds limit of {MaxFileSize / (1024 * 1024)}Mb" });
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { error = $"File type not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}" });
            }

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File uploaded: {FileName} (Original: {OriginalFileName})",
                uniqueFileName, file.FileName);

            return Ok(new
            {
                message = "File uploaded successfully",
                fileId = uniqueFileName,
                originalFileName = file.FileName,
                fileSize = file.Length,
                contentType = file.ContentType,
                uploadedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { error = "An error occurred while uploading the file" });
        }
    }

    [HttpGet("status/{fileId}")]
    public IActionResult GetFileStatus(string fileId)
    {
        var filePath = Path.Combine(_uploadPath, fileId);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { error = "File not found" });
        }

        var fileInfo = new FileInfo(filePath);

        return Ok(new
        {
            fileId,
            exists = true,
            fileSize = fileInfo.Length,
            createdAt = fileInfo.CreationTimeUtc
        });
    }

}