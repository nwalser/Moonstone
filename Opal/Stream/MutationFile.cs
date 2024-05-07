namespace Opal.Stream;

public record MutationFile
{
    private const string FileExtension = "bin";
    
    public Guid SessionId { get; init; }
    public Guid FileId { get; init; }
    
    public string GetFilename()
    {
        return $"{SessionId}_{FileId}";
    }

    public string GetFilenameWithExtension()
    {
        return $"{GetFilename()}.{FileExtension}";
    }

    public static string SearchPattern(Guid sessionId)
    {
        return $"{sessionId}_*.{FileExtension}";
    }
    
    public static MutationFile ParseFromFilename(string filename)
    {
        var splitted = filename.Split("_");
        
        return new MutationFile()
        {
            SessionId = Guid.Parse(splitted[0]),
            FileId = Guid.Parse(splitted[1]),
        };
    }

    public static MutationFile CreateNew(Guid sessionId)
    {
        return new MutationFile()
        {
            SessionId = sessionId,
            FileId = Guid.NewGuid(),
        };
    }
}
