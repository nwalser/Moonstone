using DistributedSessions.Mutations;

namespace DistributedSessions.MutationHandlers;

public interface IMutationHandler<in TMutation> where TMutation : Mutation
{
    public void Handle(TMutation mutation);
}