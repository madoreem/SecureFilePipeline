namespace SecureFilePipeline.Db.Entities;

public class FileMetadata
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = "";
    public string FileType { get; set; } = "";
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
}