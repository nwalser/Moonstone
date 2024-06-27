using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Channels;
using Moonstone.Database.Deltas;
using Moonstone.Database.Exceptions;
using ProtoBuf;

namespace Moonstone.Database;

// todo: improve error handling
public abstract class Database : IDatabase, IDisposable
{
    private const long MaxSize = 10 * 1024 * 1024;

    public long Id { get; private set; }
    public string? RootFolder { get; private set; }
    public string? LogFolder { get; private set; }
    public string? Session { get; private set; }
    public DatabaseMetadata? Metadata { get; private set; }
    
    protected abstract Dictionary<int, Type> TypeMap { get; }
    private FileInfo? _currentLogFile;

    private readonly Dictionary<Guid, Document> _documents = new();
    private readonly HashSet<Guid> _deleted = [];
    private readonly Dictionary<string, long> _filePointers = new();
    private FileSystemWatcher? _watcher;

    private readonly Channel<FileSystemEventArgs> _changedFiles = Channel.CreateUnbounded<FileSystemEventArgs>();
    private Task? _backgroundTask;
    private CancellationTokenSource _cts = new();

    private readonly BehaviorSubject<DateTime> _lastUpdate = new(DateTime.MinValue);
    public BehaviorSubject<DateTime> LastUpdate => _lastUpdate;

    public void Create(string path, string session)
    {
        if (Directory.EnumerateFileSystemEntries(path).Any())
            throw new DirectoryNotEmptyException();

        // create initial files
        Directory.CreateDirectory(Path.Join(path, "logs"));

        var metadata = new DatabaseMetadata()
        {
            Type = GetType().FullName ?? throw new InvalidOperationException(),
            Created = DateTime.UtcNow,
        };

        var json = JsonSerializer.Serialize(metadata);
        File.WriteAllText(Path.Join(path, "metadata.json"), json);
        
        Open(path, session);
        OnAfterCreating();
    }
    
    public void Close()
    {
        if (_watcher is not null)
            _watcher.EnableRaisingEvents = false;

        OnAfterClosing();
    }

    public void Open(string path, string session)
    {
        RootFolder = path;
        Session = session;
        LogFolder = Path.Join(RootFolder, "logs");
        Id = path.GetHashCode();

        // check metadata
        var metadataFile = Path.Join(path, "metadata.json");
        if (!File.Exists(metadataFile)) throw new DatabaseNotFoundException();
        var json = File.ReadAllText(metadataFile);
        var metadata = JsonSerializer.Deserialize<DatabaseMetadata>(json) ?? throw new DatabaseNotFoundException();
        if (metadata.Type != GetType().FullName)
            throw new WrongDatabaseTypeException();
        Metadata = metadata;
        
        _watcher = new FileSystemWatcher()
        {
            Path = LogFolder,
            NotifyFilter = NotifyFilters.LastWrite,
            IncludeSubdirectories = true,
            EnableRaisingEvents = false,
        };

        // get last log file
        _currentLogFile = Directory
            .EnumerateFiles(LogFolder, $"{Session}_*.bin")
            .Select(p => new FileInfo(p))
            .Where(p => p.Length < MaxSize)
            .MinBy(f => f.Length) ?? NewLogFile();

        // setup file system watcher
        _watcher.Changed += (o, args) => _changedFiles.Writer.TryWrite(args);
        _watcher.EnableRaisingEvents = true;

        // rebuild current state
        var logFiles = Directory.EnumerateFiles(LogFolder)
            .Select(f => new FileInfo(f));

        foreach (var logFile in logFiles)
            RescanFile(logFile);

        // init background worker
        _cts = new CancellationTokenSource();
        _backgroundTask = Task.Run(() => BackgroundTask(_cts.Token));
        
        OnAfterOpening();
    }

    
    protected virtual void OnAfterCreating() { }
    protected virtual void OnAfterClosing() { }
    protected virtual void OnAfterOpening() { }

    
    private async Task BackgroundTask(CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            var change = await _changedFiles.Reader.ReadAsync(ct);
            var fileInfo = new FileInfo(change.FullPath);
            RescanFile(fileInfo);
        }
    }

    private void RescanFile(FileInfo file)
    {
        // test if file is already known
        if (_filePointers.TryGetValue(file.FullName, out var filePointer) && filePointer >= file.Length)
            return;

        using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        while (fs.Position < fs.Length)
        {
            var delta = Serializer.DeserializeWithLengthPrefix<IDelta>(fs, PrefixStyle.Base128);
            ApplyDelta(delta);
        }

        _filePointers[file.FullName] = fs.Position;
        Console.WriteLine($"Rescanned: {file.FullName}");
    }

    private void ApplyDelta(IDelta delta)
    {
        lock (_documents)
        {
            switch (delta)
            {
                case Update update:
                    ApplyUpdate(update);
                    break;
                case Delete delete:
                    ApplyDelete(delete);
                    break;
            }
        }
    }

    private void ApplyDelete(Delete delete)
    {
        _deleted.Add(delete.RowId);
        _documents.Remove(delete.RowId);

        _lastUpdate.OnNext(DateTime.UtcNow);
    }

    private void ApplyUpdate(Update update)
    {
        if (!TypeMap.TryGetValue(update.TypeId, out var type))
            return;
        
        if (_deleted.Contains(update.RowId))
            return;

        if (_documents.TryGetValue(update.RowId, out var existingDocument) &&
            existingDocument.LastWrite >= update.Timestamp)
            return;

        var document = (Document)JsonSerializer.Deserialize(update.Json, type)!;
        document.LastWrite = update.Timestamp;
        _documents[update.RowId] = document;

        _lastUpdate.OnNext(DateTime.UtcNow);
    }

    private FileInfo NewLogFile()
    {
        var path = Path.Join(LogFolder, $"{Session}_{Guid.NewGuid()}.bin");
        return new FileInfo(path);
    }

    public void Update(Document document) => Update([document]);

    public void Update(IEnumerable<Document> documents)
    {
        var deltas = documents.Select(document =>
        {
            return new Update
            {
                Timestamp = DateTime.UtcNow,
                TypeId = TypeMap.Single(t => t.Value == document.GetType()).Key,
                RowId = document.Id,
                Json = JsonSerializer.Serialize(document, document.GetType()),
            };
        });

        WriteDeltas(deltas);
    }

    public IEnumerable<Document> Enumerate()
    {
        return _documents.Select(d => d.Value);
    }

    public IEnumerable<TType> Enumerate<TType>() where TType : Document
    {
        return _documents.Select(v => v.Value)
            .Where(t => t.GetType() == typeof(TType))
            .Cast<TType>();
    }

    public void Remove(Document document) => Remove([document]);

    public void Remove(IEnumerable<Document> documents)
    {
        var deltas = documents.Select(document =>
        {
            return new Delete
            {
                Timestamp = DateTime.UtcNow,
                TypeId = TypeMap.Single(t => t.Value == document.GetType()).Key,
                RowId = document.Id,
            };
        });

        WriteDeltas(deltas);
    }

    private void WriteDeltas(IEnumerable<IDelta> deltas)
    {
        if (_currentLogFile is null) throw new InvalidOperationException();

        var fs = _currentLogFile.Open(FileMode.Append, FileAccess.Write, FileShare.Read);

        foreach (var delta in deltas)
        {
            // switch log file
            if (fs.Length >= MaxSize)
            {
                fs.Dispose();
                _currentLogFile = NewLogFile();
                fs = _currentLogFile.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
            }

            Serializer.SerializeWithLengthPrefix(fs, delta, PrefixStyle.Base128);
            ApplyDelta(delta);
        }

        fs.Dispose();
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _backgroundTask?.Dispose();
        _cts.Dispose();
        _lastUpdate.Dispose();
    }
}