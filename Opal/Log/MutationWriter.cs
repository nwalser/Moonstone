using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Opal.Mutations;
using ProtoBuf;

namespace Opal.Log;

public class MutationWriter
{
    private const PrefixStyle PrefixStyle = ProtoBuf.PrefixStyle.Base128;
    private const int FieldNumber = 0;
    private const int RolloverFileSize = 1024 * 256;

    public required string Folder { get; init; }
    public required Guid SessionId { get; init; }
    
    public required MutationFile CurrentFile { get; set; }

    private readonly ConcurrentQueue<MutationEnvelope<MutationBase>> _mutationsToWrite;

    private string GetFilePath(MutationFile file) => Path.Join(Folder, file.GetFilenameWithExtension());

    
    public MutationWriter()
    {
        _mutationsToWrite = new ConcurrentQueue<MutationEnvelope<MutationBase>>();
    }

    
    public void Append(MutationEnvelope<MutationBase> entry)
    {
        _mutationsToWrite.Enqueue(entry);
    }

    public async Task ProcessWork(CancellationToken ct = default)
    {
        while (_mutationsToWrite.Any())
        {
            await using var stream = File.Open(GetFilePath(CurrentFile), FileMode.Append, FileAccess.Write, FileShare.Read);

            while (_mutationsToWrite.TryDequeue(out var entry))
            {
                if(ct.IsCancellationRequested)
                    break;
                
                if (stream.Length >= RolloverFileSize)
                {
                    CurrentFile = MutationFile.CreateNew(SessionId);
                    break;
                }
            
                Serializer.SerializeWithLengthPrefix(stream, entry, PrefixStyle, FieldNumber);
            }
            
            await stream.FlushAsync(ct);
        }
    }

    public static MutationWriter InitializeFrom(string folder, Guid sessionId)
    {
        var file = Directory
            .EnumerateFiles(folder, MutationFile.SearchPattern(sessionId))
            .Where(f => new FileInfo(f).Length < RolloverFileSize)
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Select(f => MutationFile.ParseFromFilename(f))
            .FirstOrDefault();
        
        if(file is null)
            file = MutationFile.CreateNew(sessionId);
        
        return new MutationWriter()
        {
            SessionId = sessionId,
            Folder = folder,
            CurrentFile = file,
        };
    }
}