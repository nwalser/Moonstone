using ProtoBuf;

namespace DistributedSessions;

public class MutationWriter
{
    private static readonly int MaxMutationsPerFile = 10_000;
    
    private int? _fileCounter;
    private int? _mutations;
    
    private readonly string _workspaceFolder;
    private readonly Guid _sessionId;
    
    private string CurrentSessionPath => Path.Join(_workspaceFolder, _sessionId.ToString());
    private string CurrentFilePath => Path.Join(_workspaceFolder, _sessionId.ToString(), $"{_fileCounter}.bin");

    
    public MutationWriter(string workspaceFolder, Guid sessionId)
    {
        _workspaceFolder = workspaceFolder;
        _sessionId = sessionId;
    }

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(CurrentSessionPath);
        
        _fileCounter = Directory
            .EnumerateFiles(CurrentSessionPath, "*.bin")
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
        await using var stream = File.OpenRead(CurrentFilePath);
        var mutations =  Serializer.DeserializeItems<Mutation>(stream, PrefixStyle.Base128, 0).ToList();        
        
        _mutations = mutations.Count;
    }
    
    public async Task StoreMutation(Mutation mutation)
    {
        if (_fileCounter is null || _mutations is null)
            throw new Exception("Mutation writer not initialized");

        // mutation maximum reached
        if (_mutations >= MaxMutationsPerFile)
        {
            _fileCounter++;
            _mutations = 0;
        }
        
        await using var fileStream = File.Open(CurrentFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        Serializer.SerializeWithLengthPrefix(fileStream, mutation, PrefixStyle.Base128, 0);
        await fileStream.FlushAsync();
        fileStream.Close();
        
        _mutations++;
    }
}