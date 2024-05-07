using Opal.Mutations;
using Opal.Stream;

namespace Opal;

public class Projection : IProjection
{
    public int Counter { get; set; }
    
    
    public void ApplyMutation(MutationBase mutation)
    {
        Counter++;
    }
}