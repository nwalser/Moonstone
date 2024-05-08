using Opal.Mutations;

namespace Opal.Projection;

public interface IProjection
{
    public void ApplyMutation(MutationBase mutation);
}