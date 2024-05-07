using System.Collections.Concurrent;
using ProtoBuf;

namespace Opal.Stream;

public class MutationWriter<TMutation>
{
    private const PrefixStyle PrefixStyle = ProtoBuf.PrefixStyle.Base128;
    private const int FieldNumber = 0;
    private const int RolloverFileSize = 256 * 1024;

    private readonly ConcurrentQueue<MutationEnvelope<TMutation>> _mutationsToWrite;
    
    private readonly string _folder;
    private readonly Guid _sessionId;
    private MutationFile? _currentFile;
    
    private string GetFilePath(MutationFile file) => Path.Join(_folder, file.GetFilenameWithExtension());

    
    public MutationWriter(string folder, Guid sessionId)
    {
        _folder = folder;
        _sessionId = sessionId;
        _mutationsToWrite = new ConcurrentQueue<MutationEnvelope<TMutation>>();
    }

    public void Initialize()
    {
        var file = Directory
            .EnumerateFiles(_folder, MutationFile.SearchPattern(_sessionId))
            .Where(f => new FileInfo(f).Length < RolloverFileSize)
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Select(f => MutationFile.ParseFromFilename(f))
            .FirstOrDefault();
        
        if(file is null)
            file = MutationFile.CreateNew(_sessionId);

        _currentFile = file;
    }
    
    public void Append(MutationEnvelope<TMutation> entry)
    {
        _mutationsToWrite.Enqueue(entry);
    }

    public async Task ProcessWork(CancellationToken ct = default)
    {
        if (_currentFile is null)
            throw new InvalidOperationException();
        
        while (!_mutationsToWrite.IsEmpty)
        {
            await using var stream = File.Open(GetFilePath(_currentFile), FileMode.Append, FileAccess.Write, FileShare.Read);

            while (!_mutationsToWrite.IsEmpty)
            {
                if(ct.IsCancellationRequested)
                    break;
                
                if (stream.Length >= RolloverFileSize)
                {
                    _currentFile = MutationFile.CreateNew(_sessionId);
                    break;
                }
            
                if(_mutationsToWrite.TryDequeue(out var entry))
                    Serializer.SerializeWithLengthPrefix(stream, entry, PrefixStyle, FieldNumber);
            }
            
            await stream.FlushAsync(ct);
        }
    }
}