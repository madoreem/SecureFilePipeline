namespace SecureFilePipeline.Shared;
public class FileDebouncer
{
    private readonly TimeSpan _debounceTime;
    private readonly Dictionary<string, DateTime> _recentFiles = new();
    private readonly object _lock = new();

    public FileDebouncer(TimeSpan debounceTime)
    {
        _debounceTime = debounceTime;
    }

    public bool ShouldProcess(string fileName)
    {
        lock (_lock)
        {
            if (_recentFiles.TryGetValue(fileName, out var lastTime))
            {
                if (DateTime.UtcNow - lastTime < _debounceTime)
                    return false;
            }

            _recentFiles[fileName] = DateTime.UtcNow;

            var cutoff = DateTime.UtcNow - TimeSpan.FromMinutes(10);
            foreach (var key in new List<string>(_recentFiles.Keys))
            {
                if (_recentFiles[key] < cutoff)
                    _recentFiles.Remove(key);
            }

            return true;
        }
    }
}