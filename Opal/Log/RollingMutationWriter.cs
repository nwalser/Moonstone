namespace Opal.Log;

public class RollingMutationWriter
{
    private const int RolloverFileSize = 1024 * 256;

    public required string Folder { get; init; }
    public required Guid SessionId { get; init; }
    
    public required MutationFile CurrentFile { get; set; }

    
    public void Append<TEntry>(TEntry entry, CancellationToken ct = default)
    {
        // mutation maximum per file reached
        var currentFile = GetFilePath(CurrentFile);
        var fileSize = 0L;
        
        if(File.Exists(currentFile))
            fileSize = new FileInfo(currentFile).Length;

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


    public static RollingMutationWriter InitializeFrom(string folder, Guid sessionId, CancellationToken ct = default)
    {
        var file = Directory
            .EnumerateFiles(folder, MutationFile.SearchPattern(sessionId, LockState.Open))
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Select(f => MutationFile.ParseFromFilename(f))
            .FirstOrDefault();
        
        if(file is null)
            file = MutationFile.CreateNew(sessionId);
        
        return new RollingMutationWriter()
        {
            SessionId = sessionId,
            Folder = folder,
            CurrentFile = file,
        };
    }

    private string GetFilePath(MutationFile file) => Path.Join(Folder, file.GetFilenameWithExtension());
}