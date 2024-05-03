namespace Opal.Log;

public class RollingMutationWriter<TEntry>
{
    private const string FileExtension = "bin";
    private const int RolloverFileSize = 1024 * 256;

    public required string Folder { get; init; }
    public required Guid SessionId { get; init; }
    
    public required MutationFile CurrentFile { get; set; }

    
    public void Append(TEntry entry, CancellationToken ct = default)
    {
        // mutation maximum per file reached
        var fileSize = new FileInfo(GetFilePath(CurrentFile)).Length;

        if (fileSize >= RolloverFileSize)
        {
            var currentFilePath = GetFilePath(CurrentFile);
            
            CurrentFile.LockFile();
            var lockedFilePath = GetFilePath(CurrentFile);
            
            // mark file as closed
            File.Move(currentFilePath, lockedFilePath);

            CurrentFile = MutationFile.CreateNew(SessionId);
        }

        using var writer = LogWriter.Open(GetFilePath(CurrentFile));
        writer.Append(entry);
    }


    public static RollingMutationWriter<TEntry> InitializeFrom(string folder, Guid sessionId, CancellationToken ct = default)
    {
        var file = Directory
            .EnumerateFiles(folder, $"{sessionId}_*_{LockState.Open}.{FileExtension}")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Select(f => MutationFile.ParseFromFilename(f))
            .FirstOrDefault();
        
        if(file is null)
            file = MutationFile.CreateNew(sessionId);
        
        return new RollingMutationWriter<TEntry>()
        {
            SessionId = sessionId,
            Folder = folder,
            CurrentFile = file,
        };
    }

    private string GetFilePath(MutationFile file) => Path.Join(Folder, $"{file.GetFilename()}.{FileExtension}");
}