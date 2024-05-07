using Opal.Mutations;

namespace Opal.Stream;

public interface IProjection
{
    public void ApplyMutation(MutationBase mutation);
}