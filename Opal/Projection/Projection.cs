using Opal.Mutations;

namespace Opal.Projection;

public class Projection : IProjection
{
    public int Counter { get; set; }
    
    
    public void ApplyMutation(MutationBase mutation)
    {
        Counter++;
    }
}