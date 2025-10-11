using SecureFilePipeline.MetadataService.Interfaces;
using SecureFilePipeline.MetadataService.Models;
using DocumentFormat.OpenXml.Packaging;

namespace SecureFilePipeline.MetadataService.Extractors;

public class DocxMetadataExtractor : IMetadataExtractor
{
    public bool CanHandle(string extension) => extension.Equals(".docx", StringComparison.OrdinalIgnoreCase);

    public async Task<FileMetadata> ExtractAsync(string filePath)
    {
        var info = new FileInfo(filePath);
        var metadata = new FileMetadata
        {
            FileName = info.Name,
            FileType = "DOCX",
            FileSize = info.Length,
            CreatedAt = info.CreationTimeUtc
        };

        await Task.Run(() =>
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            var props = doc.PackageProperties;

            metadata.Properties["Title"] = props.Title ?? "";
            metadata.Properties["Author"] = props.Creator ?? "";
            metadata.Properties["Created"] = props.Created?.ToString() ?? "";
            metadata.Properties["Modified"] = props.Modified?.ToString() ?? "";
        });

        return metadata;
    }
}
