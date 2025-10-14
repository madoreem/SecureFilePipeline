using Microsoft.EntityFrameworkCore;
using SecureFilePipeline.MetadataService.Interfaces;
using SecureFilePipeline.MetadataService.Extractors;
using SecureFilePipeline.Shared;
using SecureFilePipeline.Db;


namespace SecureFilePipeline.MetadataService;

public class Program
{
    private const string _scannedPath = "/app/scanned";
    private const string _processedPath = "/app/processed";
    private static readonly FileDebouncer _debouncer = new FileDebouncer(TimeSpan.FromSeconds(2));

    private static readonly List<IMetadataExtractor> _extractors = new()
    {
        new PdfMetadataExtractor(),
        new DocxMetadataExtractor(),
        new TxtMetadataExtractor(),
        new ImageMetadataExtractor()
    };

    public static async Task Main()
    {
        Directory.CreateDirectory(_scannedPath);
        Directory.CreateDirectory(_processedPath);

        var connectionString = DbConfig.GetConnectionString();
        var optionsBuilder = new DbContextOptionsBuilder<FileMetadataContext>();
        optionsBuilder.UseNpgsql(connectionString);
        var dbOptions = optionsBuilder.Options;

        var watcher = new FileSystemWatcher(_scannedPath)
        {
            Filter = "*.*",
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size
        };

        watcher.Created += async (s, e) => await OnNewFileAsync(e.FullPath, dbOptions);
        watcher.Changed += async (s, e) => await OnNewFileAsync(e.FullPath, dbOptions);

        await Task.Delay(-1);
    }

    private static async Task OnNewFileAsync(string filePath, DbContextOptions<FileMetadataContext> dbOptions)
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
        try
        {
            await using var dbContext = new FileMetadataContext(dbOptions);
            await dbContext.Database.EnsureCreatedAsync();

            dbContext.Files.Add(metadata);
            await dbContext.SaveChangesAsync();

            Console.WriteLine($"[META] {metadata.FileName} saved to DB with Id={metadata.Id}");

            var destPath = Path.Combine(_processedPath, fileName);
            File.Move(filePath, destPath, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to save {metadata.FileName}: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"[INNER] {ex.InnerException.Message}");
        }
    }
}