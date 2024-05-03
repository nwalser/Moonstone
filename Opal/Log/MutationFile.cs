namespace Opal.Log;

public record MutationFile
{
    public Guid SessionId { get; init; }
    public Guid FileId { get; init; }
    public LockState Lock { get; set; }
    
    public string GetFilename()
    {
        return $"{SessionId}_{FileId}_{Lock}";
    }

    public void LockFile()
    {
        Lock = LockState.Closed;
    }
    
    public static MutationFile ParseFromFilename(string filename)
    {
        var splitted = filename.Split("_");
        
        return new MutationFile()
        {
            SessionId = Guid.Parse(splitted[0]),
            FileId = Guid.Parse(splitted[1]),
            Lock = Enum.Parse<LockState>(splitted[2])
        };
    }
}
