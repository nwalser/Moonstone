namespace Opal.Cache;

public class FilePointer
{
    public Guid FileId { get; init; }
    public int CurrentRow { get; set; }
}