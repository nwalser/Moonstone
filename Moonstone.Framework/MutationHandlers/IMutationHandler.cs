using Moonstone.Framework.Mutations;

namespace Moonstone.Framework.MutationHandlers;

public interface IMutationHandler<in TMutation, TModel> where TMutation : Mutation
{
    public void Handle(TMutation mutation);
}