namespace SecureFilePipeline.MetadataService.Models;

public class FileMetadata
{
    public string FileName { get; set; } = "";
    public string FileType { get; set; } = "";
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
}