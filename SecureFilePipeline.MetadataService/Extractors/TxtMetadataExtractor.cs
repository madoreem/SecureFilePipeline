using SecureFilePipeline.MetadataService.Interfaces;
using SecureFilePipeline.MetadataService.Models;

namespace SecureFilePipeline.MetadataService.Extractors;

public class TxtMetadataExtractor : IMetadataExtractor
{
    public bool CanHandle(string extension)
    {
        return extension.Equals(".txt", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<FileMetadata> ExtractAsync(string filePath) 
    {
        var info = new FileInfo(filePath);
        var metadata = new FileMetadata
        {
            FileName = info.Name,
            FileType = "TXT",
            FileSize = info.Length,
            CreatedAt = info.CreationTimeUtc
        };

        string[] lines = await File.ReadAllLinesAsync(filePath);
        metadata.Properties["LineCount"] = lines.Length.ToString();
        metadata.Properties["Preview"] = string.Join(' ', lines.Take(3));

        return metadata;
    }
}