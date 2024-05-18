using System.Globalization;
using System.Reactive.Linq;

namespace Amber;

public class Workspace : IWorkspace
{
    private readonly string _path;
    private readonly string _session;
    private readonly List<DocumentReader> _documentCollections;
    
    private List<Document> _documents = [];

    private readonly Queue<FileSystemEventArgs> _changedFiles = new();

    private Task? _backgroundTask;
    private CancellationTokenSource? _backgroundTaskCts;
    private FileSystemWatcher? _fileSystemWatcher;

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public Workspace(string path, string session, List<IHandler> documentCollections)
    {
        _path = path;
        _session = session;
        _documentCollections = documentCollections
            .Select(h => new DocumentReader()
            {
                Handler = h,
                Session = session,
            })
            .ToList();
    }

    public static void Delete(string path)
    {
        if (!Directory.Exists(path)) throw new Exception(); // todo better exceptions
        
        Directory.Delete(path, recursive: true);
    }
    
    public static async Task<Workspace> Open(string path, string session, List<IHandler> handlers)
    {
        if (!Directory.Exists(path)) throw new Exception(); // todo better exceptions
        
        var workspace = new Workspace(path, session, handlers);
        await workspace.Init();
        return workspace;
    }

    public async Task Close()
    {
        if(_backgroundTaskCts is not null)
            await _backgroundTaskCts.CancelAsync();
        
        if(_backgroundTask is not null)
            await _backgroundTask;

        if (_fileSystemWatcher is not null)
            _fileSystemWatcher.EnableRaisingEvents = false;
    }

    public static async Task<Workspace> Create(string path, string session, List<IHandler> handlers)
    {
        if (Directory.Exists(path)) throw new Exception(); // todo better exceptions
        
        Directory.CreateDirectory(path);

        var workspace = new Workspace(path, session, handlers);
        await workspace.Init();
        return workspace;
    }
    
    // todo: implement deletion of document
    
    private async Task Init()
    {
        // setup file system watcher
        _fileSystemWatcher = new FileSystemWatcher
        {
            Path = _path,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite,
        };
        _fileSystemWatcher.Created += (_, e) => _changedFiles.Enqueue(e);
        _fileSystemWatcher.Changed += (_, e) => _changedFiles.Enqueue(e);
        
        // load document metadata
        await CacheAllDocuments();
        
        // start background task
        _backgroundTaskCts = new CancellationTokenSource();
        _backgroundTask = Task.Run(async () => await ProcessBackgroundWork(_backgroundTaskCts.Token));
    }
    
    private async Task CacheAllDocuments()
    {
        // load all documents from disk
        var documentTypeFolders = Directory.EnumerateDirectories(_path);
        foreach (var documentTypeFolder in documentTypeFolders)
        {
            var documentFolders = Directory.EnumerateDirectories(documentTypeFolder);
            foreach (var documentFolder in documentFolders)
            {
                await CacheDocument(documentFolder);
            }
        }
    }

    private async Task ProcessBackgroundWork(CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await _semaphore.WaitAsync(ct);

                // process all changed files
                while (!ct.IsCancellationRequested && _changedFiles.TryDequeue(out var changedFile))
                    await ProcessChangedFile(changedFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
            
            await Task.Delay(100, ct);
        }
    }

    private async Task ProcessChangedFile(FileSystemEventArgs e)
    {
        // do not handle folder changes
        if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
            return;
        
        var separators = new char[] {
            Path.DirectorySeparatorChar,  
            Path.AltDirectorySeparatorChar  
        };
        
        var subpath = Path.GetRelativePath(_path, e.FullPath);
        var subpathSegments = subpath.Split(separators, StringSplitOptions.RemoveEmptyEntries); // todo implement better path splitting

        // update or index document
        if (subpathSegments.Length == 3)
        {
            var documentFolder = Path.GetDirectoryName(e.FullPath) ?? throw new Exception();
            await CacheDocument(documentFolder);
        }
    }

    private async Task CacheDocument(string documentFolder) // todo: do not pass path but type and id
    {
        var separators = new[] {
            Path.DirectorySeparatorChar,  
            Path.AltDirectorySeparatorChar  
        };
        
        var subpathSegments = documentFolder.Split(separators, StringSplitOptions.RemoveEmptyEntries); // todo implement better path splitting

        var documentId = Guid.Parse(subpathSegments[^1], CultureInfo.InvariantCulture);
        var documentTypeId = Convert.ToInt32(subpathSegments[^2], CultureInfo.InvariantCulture);
        var documentReader = GetReaderForTypeId(documentTypeId);
        
        var documentValue = await documentReader.Read(documentFolder);

        // if document is not cached already load into cache
        if(_documents.All(d => d.Id != documentId))
            _documents.Add(new Document(documentId, documentValue));
        
        // update document value
        var document = _documents.Single(d => d.Id == documentId);
        document.UpdateValue(documentValue);
    }

    private DocumentReader GetReaderForType(Type type) =>
        _documentCollections.Single(d => d.Handler.DocumentType == type);

    private DocumentReader GetReaderForTypeId(int typeId) =>
        _documentCollections.Single(d => d.Handler.DocumentTypeId == typeId);
    
    private string BuildPath(Type type, Guid documentId)
    {
        var documentReader = GetReaderForType(type);
        return Path.Join(_path, documentReader.Handler.DocumentTypeId.ToString(CultureInfo.InvariantCulture), documentId.ToString());
    }
    
    public async Task Create<TDocument>(Guid? id = default)
    {
        try
        {
            await _semaphore.WaitAsync();

            var documentId = id ?? Guid.NewGuid();
            var documentType = typeof(TDocument);
            
            var documentReader = GetReaderForType(documentType);
            var documentPath = BuildPath(typeof(TDocument), documentId);
            
            documentReader.Create(documentPath);

            await CacheDocument(documentPath);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public TDocument Load<TDocument>(Guid id)
    {
        return (TDocument)_documents.Single(d => d.Id == id).Value;
    }

    public IObservable<TDocument> Observe<TDocument>(Guid id)
    {
        return _documents.Single(d => d.Id == id).ValueObservable.Cast<TDocument>(); // todo: implement working cast
    }

    public async Task ApplyMutation<TDocument>(Guid documentId, object mutation)
    {
        try
        {
            await _semaphore.WaitAsync();
            
            var documentType = typeof(TDocument);

            var documentReader = GetReaderForType(documentType);
            var documentPath = BuildPath(documentType, documentId);
            
            // update document values in ram
            var document = _documents.Single(d => d.Id == documentId);
            documentReader.Handler.ApplyMutation(document.Value, mutation);
            document.UpdateValue(document.Value);
            
            // write to disk
            await documentReader.Append(documentPath, mutation);
            
            // recache
            await CacheDocument(documentPath);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task Delete<TDocument>(Guid documentId)
    {
        try
        {
            await _semaphore.WaitAsync();
            
            var documentType = typeof(TDocument);

            var documentReader = GetReaderForType(documentType);
            var documentPath = BuildPath(documentType, documentId);
            
            documentReader.Delete(documentPath);
            _documents.Remove(_documents.Single(d => d.Id == documentId));

            // todo implement deletion log
        }
        finally
        {
            _semaphore.Release();
        }
    }
}