namespace Opal.Cache;

public class FilePointer
{
    public Guid FileId { get; init; }
    public long ReadBytes { get; set; }

    public static FilePointer Create(Guid fileId)
    {
        return new FilePointer()
        {
            FileId = fileId,
            ReadBytes = 0,
        };
    }
}