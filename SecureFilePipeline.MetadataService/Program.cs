using SecureFilePipeline.MetadataService.Interfaces;
using SecureFilePipeline.MetadataService.Models;
using SecureFilePipeline.MetadataService.Extractors;
using SecureFilePipeline.Shared;

namespace SecureFilePipeline.MetadataService;

public class Program
{
    private const string _scannedPath = "/app/scanned";
    private const string _metadataPath = "/app/processed";
    private static readonly FileDebouncer _debouncer = new FileDebouncer(TimeSpan.FromSeconds(2));

    private static readonly List<IMetadataExtractor> _extractors = new()
    {
        new PdfMetadataExtractor(),
        new DocxMetadataExtractor()
    };

    public static async Task Main()
    {
        Directory.CreateDirectory(_scannedPath);
        Directory.CreateDirectory(_metadataPath);

        var watcher = new FileSystemWatcher(_scannedPath)
        {
            Filter = "*.*",
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size
        };

        watcher.Created += async (s, e) => await OnNewFileAsync(e.FullPath);
        watcher.Changed += async (s, e) => await OnNewFileAsync(e.FullPath);

        await Task.Delay(-1);
    }

    private static async Task OnNewFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        if (!_debouncer.ShouldProcess(fileName))
            return;

        await Task.Delay(1000);

        if (!File.Exists(filePath))
            return;

        var ext = Path.GetExtension(filePath);
        var extractor = _extractors.FirstOrDefault(e => e.CanHandle(ext));
        if (extractor == null)
        {
            Console.WriteLine($"[SKIP] No extractor for {ext}");
            return;
        }

        var metadata = await extractor.ExtractAsync(filePath);

        Console.WriteLine($"[META] {fileName}:");
        foreach (var kv in metadata.Properties)
            Console.WriteLine($"   - {kv.Key}: {kv.Value}");
        
        var dest = Path.Combine(_metadataPath, Path.GetFileName(filePath));
        File.Move(filePath, dest, true);
    }
}