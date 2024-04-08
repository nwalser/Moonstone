using DistributedSessions.Mutations;
using ProtoBuf;

namespace DistributedSessions;

public class MutationWriter
{
    private static int MaxMutationsPerFile = 10_000;
    
    private int? _fileCounter;
    private int? _mutations;
    
    private readonly string _workspaceFolder;
    private readonly Guid _sessionId;
    
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
            _fileCounter++;
            _mutations = 0;
        }
        
        await WriteMutation(mutation);
        _mutations++;
    }

    private async Task WriteMutation(Mutation mutation)
    {
        if (_fileCounter is null || _mutations is null)
            throw new Exception();

        if (!File.Exists(CurrentFilePath))
        {
            await using var test = File.Create(CurrentFilePath);
        }

        var tempPath = CurrentFilePath + ".tmp";
        // copy into tempfile
        File.Copy(CurrentFilePath, tempPath);
        
        await using var fileStream = File.Open(tempPath, FileMode.Append, FileAccess.Write, FileShare.Read);
        Serializer.SerializeWithLengthPrefix(fileStream, mutation, PrefixStyle.Base128, 0);

        await fileStream.FlushAsync();
        fileStream.Close();
        Thread.Sleep(100);
        File.Replace(tempPath, CurrentFilePath, null);
    }

    private async Task<List<Mutation>> ReadMutations()
    {
        await using var stream = File.OpenRead(CurrentFilePath);
        return Serializer.DeserializeItems<Mutation>(stream, PrefixStyle.Base128, 0).ToList();
    }
}