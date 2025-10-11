using SecureFilePipeline.MetadataService.Interfaces;
using SecureFilePipeline.MetadataService.Models;
using UglyToad.PdfPig;

namespace SecureFilePipeline.MetadataService.Extractors;

public class PdfMetadataExtractor : IMetadataExtractor
{
    public bool CanHandle(string extension)
    {
        return extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<FileMetadata> ExtractAsync(string filePath)
    {
        var info = new FileInfo(filePath);
        var metadata = new FileMetadata
        {
            FileName = info.Name,
            FileType = "PDF",
            FileSize = info.Length,
            CreatedAt = info.CreationTimeUtc
        };

        await Task.Run(() =>
        {
            using var pdf = PdfDocument.Open(filePath);
            metadata.Properties["PageCount"] = pdf.NumberOfPages.ToString();
            if (pdf.Information != null)
            {
                metadata.Properties["Title"] = pdf.Information.Title ?? "";
                metadata.Properties["Author"] = pdf.Information.Author ?? "";
                metadata.Properties["Producer"] = pdf.Information.Producer ?? "";
                metadata.Properties["CreationDate"] = pdf.Information.CreationDate?.ToString() ?? "";
            }
        });

        return metadata;
    }
}