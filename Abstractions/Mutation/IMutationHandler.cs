namespace Abstractions.Mutation;

public interface IMutationHandler<in TProjection>
{
    public void Apply(TProjection projection);
}