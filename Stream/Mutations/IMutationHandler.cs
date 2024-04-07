namespace Stream.Mutations;

public interface IMutationHandler<in TMutation> where TMutation : Mutation
{
    public void Handle(TMutation mutation, Projection projection);
}