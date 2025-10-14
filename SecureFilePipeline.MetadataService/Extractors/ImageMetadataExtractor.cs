using SecureFilePipeline.MetadataService.Interfaces;
using SecureFilePipeline.MetadataService.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.IO;

namespace SecureFilePipeline.MetadataService.Extractors
{
    public class ImageMetadataExtractor : IMetadataExtractor
    {
        private static readonly string[] _extensions = { ".jpg", ".jpeg", ".png" };

        public bool CanHandle(string extension)
        {
            return _extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<FileMetadata> ExtractAsync(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            var metadata = new FileMetadata
            {
                FileName = fileInfo.Name,
                FileType = fileInfo.Extension,
                FileSize = fileInfo.Length,
                CreatedAt = fileInfo.CreationTimeUtc
            };

            await Task.Run(() =>
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);

                foreach (var dir in directories)
                {
                    foreach (var tag in dir.Tags)
                    {
                        var key = $"{dir.Name}:{tag.Name}";
                        metadata.Properties[key] = tag.Description ?? string.Empty;
                    }
                }

                var subIfdDir = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                if (subIfdDir != null)
                {
                    metadata.Properties["Width"] = subIfdDir.GetDescription(ExifDirectoryBase.TagExifImageWidth) ?? "";
                    metadata.Properties["Height"] = subIfdDir.GetDescription(ExifDirectoryBase.TagExifImageHeight) ?? "";
                }
            });

            return metadata;
        }
    }
}
