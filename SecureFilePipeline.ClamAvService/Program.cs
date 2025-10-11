using System.Net.Sockets;
using System.Text;

namespace SecureFilePipeline.ClamAvService;

public class Program
{
    private const string _uploadPath = "/app/uploads";
    private const string _scannedPath = "/app/scanned";
    private const string _quarantinePath = "/app/quarantine";
    private const string _clamAvHost = "clamav";
    private const int _clamAvPort = 3310;

    public static async Task Main()
    {
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_scannedPath);
        Directory.CreateDirectory(_quarantinePath);

        var watcher = new FileSystemWatcher(_uploadPath)
        {
            Filter = "*.*",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size,
            EnableRaisingEvents = true
        };

        watcher.Created += async (s, e) => await OnNewFileAsync(e.FullPath);
        watcher.Changed += async (s, e) => await OnNewFileAsync(e.FullPath);
        watcher.Error += (s, e) => Console.WriteLine($"[ERROR] Watcher failed: {e.GetException()?.Message}");

        await Task.Delay(-1);
    }

    private static async Task OnNewFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        if (fileName.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
            return;

        await Task.Delay(1000);

        if (!File.Exists(filePath))
            return;

        bool clean;
        try
        {
            clean = await ScanFileAsync(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Scan failed for {fileName}: {ex.Message}");
            return;
        }

        var destination = clean
            ? Path.Combine(_scannedPath, fileName)
            : Path.Combine(_quarantinePath, fileName);

        try
        {
            File.Move(filePath, destination, true);
            Console.WriteLine(clean
                    ? $"[OK] {fileName} is clean → moved to /app/scanned"
                    : $"[INFECTED] {fileName} → moved to /app/quarantine");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Moving file {fileName}: {ex.Message}");
        }
    }

    private static async Task<bool> ScanFileAsync(string filePath)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_clamAvHost, _clamAvPort);
        using var networkStream = client.GetStream();

        var command = Encoding.ASCII.GetBytes("nINSTREAM\n");
        await networkStream.WriteAsync(command, 0, command.Length);

        await using (var fileStream = File.OpenRead(filePath))
        {
            byte[] buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                byte[] size = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(bytesRead));
                await networkStream.WriteAsync(size, 0, size.Length);
                await networkStream.WriteAsync(buffer, 0, bytesRead);
            }
        }

        byte[] end = BitConverter.GetBytes(0);
        await networkStream.WriteAsync(end, 0, end.Length);

        // Read ClamAV response
        byte[] responseBuffer = new byte[4096];
        int responseBytes = await networkStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
        string response = Encoding.ASCII.GetString(responseBuffer, 0, responseBytes).Trim();

        Console.WriteLine($"[ClamAV] Response: {response}");

        // Example: "stream: OK" or "stream: Eicar-Test-Signature FOUND"
        return response.Contains("OK", StringComparison.OrdinalIgnoreCase)
            && !response.Contains("FOUND", StringComparison.OrdinalIgnoreCase);
    }
}