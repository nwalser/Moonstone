using System.Globalization;
using Amber.Documents;

namespace Amber.Ws;

public class Workspace
{
    private readonly string _folder;
    private readonly List<IHandler> _handlers;
    
    private List<Document> _documentEnvelopes = [];
    public IReadOnlyList<Document> DocumentEnvelopes => _documentEnvelopes;

    private readonly Queue<FileSystemEventArgs> _changedFiles = new();
    
    
    public Workspace(string folder, List<IHandler> handlers)
    {
        _folder = folder;
        _handlers = handlers;
    }

    public void Init()
    {
        _documentEnvelopes = LoadDocumentMetadata().ToList();
    }
    
    private IEnumerable<Document> LoadDocumentMetadata()
    {
        // setup file system watcher
        var fileSystemWatcher = new FileSystemWatcher
        {
            Path = _folder,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite,
        };
        fileSystemWatcher.Created += (_, e) => _changedFiles.Enqueue(e);
        fileSystemWatcher.Changed += (_, e) => _changedFiles.Enqueue(e);
        
        // load all documents from disk
        var documentTypeFolders = Directory.EnumerateDirectories(_folder);
        foreach (var documentTypeFolder in documentTypeFolders)
        {
            var documentTypeId = Convert.ToInt32(Path.GetFileName(documentTypeFolder));
            var documentHandler = _handlers.Single(h => h.DocumentTypeId == documentTypeId);

            var documentFolders = Directory.EnumerateDirectories(documentTypeFolder);

            foreach (var documentFolder in documentFolders)
            {
                var documentId = Guid.Parse(Path.GetFileName(documentFolder), CultureInfo.InvariantCulture);
                var documentValue = DocumentReader.Read(documentFolder, documentHandler);

                yield return new Document(documentId, documentValue);
            }
        }
    }

    private void ProcessWork(CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            // process all changed files
            while (!ct.IsCancellationRequested && _changedFiles.TryDequeue(out var changedFile))
                ProcessChangedFile(changedFile);
        }
    }

    private void ProcessChangedFile(FileSystemEventArgs e)
    {
        var subpath = Path.GetRelativePath(_folder, e.FullPath);
        var subpathSegments = subpath.Split(@"\/", StringSplitOptions.RemoveEmptyEntries);

        // directory of new document -> index it and add it to documents list
        if (subpath.Length == 2)
        {
            var documentId = Guid.Parse(subpathSegments[-1], CultureInfo.InvariantCulture);
            var documentTypeId = Convert.ToInt32(subpathSegments[-2], CultureInfo.InvariantCulture);
            var documentHandler = _handlers.Single(h => h.DocumentTypeId == documentTypeId);

            var documentValue = DocumentReader.Read(e.FullPath, documentHandler);

            _documentEnvelopes.Add(new Document(documentId, documentValue));
        }

        // file of existing document -> update document
        if (subpath.Length == 3)
        {
            var documentFolder = Path.GetDirectoryName(e.FullPath) ?? throw new Exception();
            var documentId = Guid.Parse(subpathSegments[-2], CultureInfo.InvariantCulture);
            var documentTypeId = Convert.ToInt32(subpathSegments[-3], CultureInfo.InvariantCulture);
            var documentHandler = _handlers.Single(h => h.DocumentTypeId == documentTypeId);

            var documentValue = DocumentReader.Read(documentFolder, documentHandler);

            var document = _documentEnvelopes.Single(d => d.Id == documentId && d.Value.GetType() == documentValue.GetType());
            
            document.UpdateValue(documentValue);
        }
    }
}