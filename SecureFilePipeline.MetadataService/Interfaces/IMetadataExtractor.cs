namespace SecureFilePipeline.MetadataService.Interfaces;

using SecureFilePipeline.MetadataService.Models;

public interface IMetadataExtractor
{
    bool CanHandle(string extension);
    Task<FileMetadata> ExtractAsync(string filePath);
}