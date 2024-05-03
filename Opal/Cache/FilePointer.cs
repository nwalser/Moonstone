namespace Opal.Cache;

public class FilePointer
{
    public Guid FileId { get; init; }
    public int NumberOfReadEntries { get; set; }
    public bool ReadToEnd { get; set; }

    public static FilePointer Create(Guid fileId)
    {
        return new FilePointer()
        {
            FileId = fileId,
            ReadToEnd = false,
            NumberOfReadEntries = 0,
        };
    }
}