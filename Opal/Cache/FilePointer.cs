namespace Opal.Cache;

public class FilePointer
{
    public Guid FileId { get; init; }
    public long ReadPosition { get; set; }

    public static FilePointer Create(Guid fileId)
    {
        return new FilePointer()
        {
            FileId = fileId,
            ReadPosition = 0,
        };
    }
}