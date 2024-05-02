using Abstractions.Serializer;

namespace Abstractions.Log;

public class RollingLog<TEntry>
{
    private readonly string _folder;
    private readonly int _maxEntriesPerFile;
    private readonly string _fileExtension;
    private readonly LogFile<TEntry> _logFile;

    private int _fileCounter;
    private int _entryCounter;
    

    public RollingLog(string folder, ITextSerializer<TEntry> serializer, int maxEntriesPerFile = 10_000, string fileExtension = "bin")
    {
        _folder = folder;
        _maxEntriesPerFile = maxEntriesPerFile;
        _fileExtension = fileExtension;
        _logFile = new LogFile<TEntry>(serializer);
    }

    public void Initialize(CancellationToken ct = default)
    {
        var fileCounter = Directory
            .EnumerateFiles(_folder, $"*.{_fileExtension}")
            .Select(Path.GetFileNameWithoutExtension)
            .Select(p => int.TryParse(p, out var value) ? value : default(int?))
            .Max(i => i);

        var entryCounter = default(int?);
        
        if (fileCounter is not null)
            entryCounter = _logFile.GetNumberOfEntries(GetLogFilePath(_folder, fileCounter.Value), ct);

        _fileCounter = fileCounter ?? 0;
        _entryCounter = entryCounter ?? 0;
    }
    
    public void Append(TEntry entry, CancellationToken ct = default)
    {
        // mutation maximum per file reached
        if (_entryCounter >= _maxEntriesPerFile)
        {
            // rollover file
            _fileCounter++;
            _entryCounter = 0;
        }

        _logFile.Append(entry, GetLogFilePath(_folder, _fileCounter), ct);
        _entryCounter++;
    }

    private string GetLogFilePath(string folder, int index) => Path.Join(folder, $"{index}.{_fileExtension}");
}