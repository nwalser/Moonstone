using DistributedSessions.Mutations;
using MsgPack.Serialization;

namespace DistributedSessions;

public class MutationWriter
{
    private static int MaxMutationsPerFile = 100;
    
    private int? _fileCounter;
    private int? _mutations;
    
    private readonly string _workspaceFolder;
    private readonly Guid _sessionId;
    
    private static readonly MessagePackSerializer<List<Mutation>> Serializer = MessagePackSerializer.Get<List<Mutation>>();
    
    private string CurrentSessionPath => Path.Join(_workspaceFolder, _sessionId.ToString());
    private string CurrentFilePath => Path.Join(_workspaceFolder, _sessionId.ToString(), _fileCounter.ToString());

    
    public MutationWriter(string workspaceFolder, Guid sessionId)
    {
        _workspaceFolder = workspaceFolder;
        _sessionId = sessionId;
    }

    public async Task Initialize()
    {
        Directory.CreateDirectory(CurrentSessionPath);
        
        _fileCounter = Directory
            .EnumerateFiles(CurrentSessionPath)
            .Select(Path.GetFileNameWithoutExtension)
            .Select(p => Convert.ToInt32(p))
            .Max(i => (int?)i);

        // if no file is in session
        if (_fileCounter is null)
        {
            _fileCounter = 0;
            _mutations = 0;
            return;
        }

        // read mutations from latest file
        var mutations = await ReadMutations();
        _mutations = mutations.Count;
    }
    
    public async Task StoreMutation(Mutation mutation)
    {
        if (_fileCounter is null || _mutations is null)
            throw new Exception("Mutation writer not initialized");

        // mutation maximum reached
        if (_mutations >= MaxMutationsPerFile)
        {
            _fileCounter += 1;
            _mutations = 0;
        }
        
        await WriteMutation(mutation);
    }

    private async Task WriteMutation(Mutation mutation)
    {
        if (_fileCounter is null || _mutations is null)
            throw new Exception();

        if (!File.Exists(CurrentFilePath))
        {
            var emptyBytes = Serializer.PackSingleObject(new CreateProjectMutation()
            {
                Occurence = DateTime.UtcNow,
                Name = "Test",
                MutationId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
            });
            await File.WriteAllBytesAsync(CurrentFilePath, emptyBytes);
        }
        
        await using var fileStream = File.Open(CurrentFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        MessagePackExtensions.AppendToFile(fileStream, mutation);
    }

    private async Task<List<Mutation>> ReadMutations()
    {
        var mutationBytes = await File.ReadAllBytesAsync(CurrentFilePath);
        return await Serializer.UnpackSingleObjectAsync(mutationBytes);
    }
}