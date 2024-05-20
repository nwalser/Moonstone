using System.Reactive.Subjects;
using Moonstone.Exceptions;

namespace Moonstone;

public class Workspace
{
    public string Location { get; }
    private readonly Dictionary<int, Type> _typeMap;
    private readonly FileSystemWatcher _watcher;

    private readonly Subject<DocumentIdentity> _externalChange;
    public IObservable<DocumentIdentity> ExternalChange => _externalChange;

    private readonly List<object> _readers;
    
    public Workspace(string location, Dictionary<int, Type> typeMap, List<object> readers)
    {
        Location = location;
        _typeMap = typeMap;
        _readers = readers;

        _externalChange = new Subject<DocumentIdentity>();
        
        _watcher = new FileSystemWatcher()
        {
            Path = Location,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite,
            IncludeSubdirectories = true,
        };

        _watcher.Created += RegisterExternalChange;
        _watcher.Changed += RegisterExternalChange;
    }

    private void RegisterExternalChange(object sender, FileSystemEventArgs args)
    {
        if (!File.Exists(args.FullPath) || (File.GetAttributes(args.FullPath) & FileAttributes.Directory) == FileAttributes.Directory) return;
        
        var relativePath = System.IO.Path.GetRelativePath(Location, args.FullPath);
        var segments = relativePath.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        var documentId = Guid.Parse(segments[^2]);
        var typeId = Convert.ToInt32(segments[^3]);
        var type = _typeMap[typeId];
        
        _externalChange.OnNext(new DocumentIdentity()
        {
            Workspace = Location,
            Id = documentId,
            TypeId = typeId,
            Type = type
        });
    }

    public IEnumerable<DocumentIdentity> EnumerateDocuments()
    {
        var typeFolders = Directory.EnumerateDirectories(Location);

        foreach (var typeFolder in typeFolders)
        {
            var typeId = Convert.ToInt32(Path.GetFileNameWithoutExtension(typeFolder));
            var type = _typeMap[typeId];
            var documentFolders = Directory.EnumerateDirectories(typeFolder);

            foreach (var documentFolder in documentFolders)
            {
                var documentId = Guid.Parse(Path.GetFileNameWithoutExtension(documentFolder));
                yield return new DocumentIdentity()
                {
                    Workspace = Location,
                    Id = documentId,
                    Type = type,
                    TypeId = typeId,
                };
            }
        }
    }

    public IEnumerable<DocumentIdentity> EnumerateDocuments<TDocument>()
    {
        return EnumerateDocuments().Where(d => d.Type == typeof(TDocument));
    }

    public DocumentIdentity GetById<TDocument>(Guid id)
    {
        return new DocumentIdentity()
        {
            Workspace = Location,
            Id = id,
            Type = typeof(TDocument),
            TypeId = _typeMap.Single(t => t.Value == typeof(TDocument)).Key
        };
    }
    
    private string GetDocumentPath(DocumentIdentity identity)
    {
        return Path.Join(Location, identity.TypeId.ToString(), identity.Id.ToString());
    }
    
    public DocumentIdentity Create<TDocument>(Guid? id = default)
    {
        var identity = new DocumentIdentity()
        {
            Workspace = Location,
            Id = id ?? Guid.NewGuid(),
            Type = typeof(TDocument),
            TypeId = _typeMap.Single(t => t.Value == typeof(TDocument)).Key
        };
        
        var folder = GetDocumentPath(identity);
        
        if (Directory.Exists(folder)) throw new DocumentAlreadyExistsException();
        
        Directory.CreateDirectory(folder);
        File.AppendAllLines(Path.Join(folder, "keep.me"), ["keep.me"]);

        return identity;
    }

    public void Delete(DocumentIdentity identity)
    {
        var folder = GetDocumentPath(identity);

        if (!Directory.Exists(folder)) throw new DocumentDoesNotExistException();
        Directory.Delete(folder, recursive: true);
    }
}
