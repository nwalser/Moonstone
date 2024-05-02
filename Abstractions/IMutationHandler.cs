namespace Abstractions;

public interface IMutationHandler<in TProjection>
{
    public void Apply(TProjection projection);
}