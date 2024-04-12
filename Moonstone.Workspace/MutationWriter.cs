using Moonstone.Framework;
using Moonstone.Framework.Stream;

namespace Moonstone.Workspace;

public class MutationWriter
{
    private static readonly int MaxMutationsPerFile = 10_000;
    
    private readonly PathProvider _paths;
    
    private int _fileCounter;
    private int _mutations;
    
    
    public MutationWriter(PathProvider paths)
    {
        _paths = paths;
    }

    public async Task Initialize(CancellationToken ct = default)
    {
        var fileCounter = Directory
            .EnumerateFiles(_paths.GetSessionMutationsFolder(), "*.nljson")
            .Select(Path.GetFileNameWithoutExtension)
            .Select(p => int.TryParse(p, out var value) ? value : default(int?))
            .Max(i => i);

        if (fileCounter is null)
        {
            // if no file is in session
            _fileCounter = 0;
            _mutations = 0;
        }
        else
        {
            // read mutations from latest file
            var mutations = await JsonNewlineFile.ReadAsync<Mutation>(_paths.GetSessionMutationsFile(fileCounter.Value), ct);
            
            _fileCounter = fileCounter.Value;
            _mutations = mutations.Count;
        }
    }

    public async Task WriteMutation(Mutation mutation)
    {
        // mutation maximum per file reached
        if (_mutations >= MaxMutationsPerFile)
        {
            // rollover file
            _fileCounter++;
            _mutations = 0;
        }

        var mutationsFile = _paths.GetSessionMutationsFile(_fileCounter);
        await JsonNewlineFile.Append(mutation, mutationsFile);
        _mutations++;
    }
}