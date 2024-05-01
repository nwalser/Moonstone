using Moonstone.Workspace.Data;

namespace Moonstone.Workspace.MutationStream;

public class RollingMutationStream : IMutationStream
{
    public required string Folder { get; init; }
    public required int MaxEntriesPerFile { get; init; }

    private int FileCounter { get; set; }
    private int EntryCounter { get; set; }

    
    public void Append(MutationEnvelope entry, CancellationToken ct = default)
    {
        // mutation maximum per file reached
        if (EntryCounter >= MaxEntriesPerFile)
        {
            // rollover file
            FileCounter++;
            EntryCounter = 0;
        }

        MutationLog.Append(entry, GetFilePath(Folder, FileCounter), ct);
        EntryCounter++;
    }


    public static RollingMutationStream? InitializeFrom(string folder, CancellationToken ct = default)
    {
        var fileCounter = Directory
            .EnumerateFiles(folder, "*.bin")
            .Select(Path.GetFileNameWithoutExtension)
            .Select(p => int.TryParse(p, out var value) ? value : default(int?))
            .Max(i => i);

        var entryCounter = default(int?);
        
        if (fileCounter is not null)
            entryCounter = MutationLog.GetNumberOfEntries(GetFilePath(folder, fileCounter.Value), ct);

        return new RollingMutationStream()
        {
            Folder = folder,
            FileCounter = fileCounter ?? 0,
            EntryCounter = entryCounter ?? 0,
            MaxEntriesPerFile = 10_000,
        };
    }

    private static string GetFilePath(string folder, int index) => Path.Join(folder, $"{index}.bin");
}