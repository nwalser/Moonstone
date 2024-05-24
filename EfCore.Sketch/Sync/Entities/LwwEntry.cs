namespace EfCore.Sketch.Sync;

public class LwwEntry
{
    public Guid Id { get; set; }
    
    public int Type { get; set; }
    public Guid ObjectId { get; set; }
    public int Field { get; set; }
    
    public long LastWrite { get; set; }
    public byte[] Value { get; set; }
}