namespace SecureFilePipeline.MetadataService.Interfaces;

using SecureFilePipeline.Db.Entities;

public interface IMetadataExtractor
{
    bool CanHandle(string extension);
    Task<FileMetadata> ExtractAsync(string filePath);
}