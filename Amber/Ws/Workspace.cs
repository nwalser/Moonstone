using System.Globalization;
using Amber.Documents;

namespace Amber.Ws;

public class Workspace
{
    private readonly string _path;
    private readonly string _session;
    private readonly List<IHandler> _handlers;
    
    private List<Document> _documents = [];
    public IReadOnlyList<Document> Documents => _documents;

    private readonly Queue<FileSystemEventArgs> _changedFiles = new();

    private Task? _backgroundTask;
    private CancellationTokenSource? _backgroundTaskCts;

    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    
    public Workspace(string path, string session, List<IHandler> handlers)
    {
        _path = path;
        _handlers = handlers;
        _session = session;
    }

    public static void Delete(string path)
    {
        if (!Directory.Exists(path)) throw new Exception(); // todo better exceptions
        
        Directory.Delete(path, recursive: true);
    }
    
    public static Workspace Open(string path, string session, List<IHandler> handlers)
    {
        if (!Directory.Exists(path)) throw new Exception(); // todo better exceptions
        
        var workspace = new Workspace(path, session, handlers);
        workspace.Init();
        return workspace;
    }

    public static Workspace Create(string path, string session, List<IHandler> handlers)
    {
        if (Directory.Exists(path)) throw new Exception(); // todo better exceptions
        
        Directory.CreateDirectory(path);

        var workspace = new Workspace(path, session, handlers);
        workspace.Init();
        return workspace;
    }


    private void Init()
    {
        // setup file system watcher
        var fileSystemWatcher = new FileSystemWatcher
        {
            Path = _path,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite,
        };
        fileSystemWatcher.Created += (_, e) => _changedFiles.Enqueue(e); // todo: fix events get called twice sometimes
        fileSystemWatcher.Changed += (_, e) => _changedFiles.Enqueue(e); // todo: fix events get called twice sometimes
        
        // load document metadata
        _documents = LoadDocumentMetadata().ToList();
        
        // start background task
        _backgroundTaskCts = new CancellationTokenSource();
        _backgroundTask = Task.Run(async () => await ProcessBackgroundWork(_backgroundTaskCts.Token));
    }

    public async Task<Document> CreateDocument<TDocument>()
    {
        try
        {
            await _semaphore.WaitAsync();

            var documentId = Guid.NewGuid();
            var handler = _handlers.Single(h => h.DocumentType == typeof(TDocument));

            var documentPath = Path.Join(_path, handler.DocumentTypeId.ToString(CultureInfo.InvariantCulture), documentId.ToString());

            DocumentReader.Create(documentPath);

            await UpdateDocument(documentPath);

            return Documents.Single(d => d.Id == documentId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ApplyMutation(Document document, object mutation)
    {
        try
        {
            await _semaphore.WaitAsync();
            
            var handler = _handlers.Single(h => h.DocumentType == document.Value.GetType());
            var documentPath = Path.Join(_path, handler.DocumentTypeId.ToString(CultureInfo.InvariantCulture), document.Id.ToString());

            await DocumentReader.Append(documentPath, _session, mutation, handler);
            await UpdateDocument(documentPath);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private IEnumerable<Document> LoadDocumentMetadata()
    {
        // load all documents from disk
        var documentTypeFolders = Directory.EnumerateDirectories(_path);
        foreach (var documentTypeFolder in documentTypeFolders)
        {
            var documentTypeId = Convert.ToInt32(Path.GetFileName(documentTypeFolder));
            var documentHandler = _handlers.Single(h => h.DocumentTypeId == documentTypeId);

            var documentFolders = Directory.EnumerateDirectories(documentTypeFolder);

            foreach (var documentFolder in documentFolders)
            {
                var documentId = Guid.Parse(Path.GetFileName(documentFolder), CultureInfo.InvariantCulture);
                var documentValue = DocumentReader.Read(documentFolder, documentHandler);

                yield return new Document(documentId, documentValue, ApplyMutation);
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
            finally
            {
                _semaphore.Release();
            }
            
            Thread.Sleep(100);
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
            await UpdateDocument(documentFolder);
        }
    }

    private async Task UpdateDocument(string documentFolder)
    {
        var separators = new char[] {
            Path.DirectorySeparatorChar,  
            Path.AltDirectorySeparatorChar  
        };
        
        var subpathSegments = documentFolder.Split(separators, StringSplitOptions.RemoveEmptyEntries); // todo implement better path splitting

        var documentId = Guid.Parse(subpathSegments[^1], CultureInfo.InvariantCulture);
        var documentTypeId = Convert.ToInt32(subpathSegments[^2], CultureInfo.InvariantCulture);
        var documentHandler = _handlers.Single(h => h.DocumentTypeId == documentTypeId);

        var documentValue = await DocumentReader.Read(documentFolder, documentHandler);

        var document = _documents.SingleOrDefault(d => d.Id == documentId);
        if (document is null)
        {
            _documents.Add(new Document(documentId, documentValue, ApplyMutation));
        }
        else
        {
            document.UpdateValue(documentValue);
        }
    }
}