using Opal.Mutations;

namespace Opal.Projection;

public class Projection : IProjection
{
    public int Counter { get; set; }
    public byte[] Data { get; set; } = Enumerable.Range(0, 1024 * 1024 * 10).Select(b => (byte)(b % 255)).ToArray();
    
    public void ApplyMutation(MutationBase mutation)
    {
        Counter++;
    }
}