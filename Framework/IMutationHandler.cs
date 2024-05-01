namespace Framework;

public interface IMutationHandler<in TProjection>
{
    public void Apply(TProjection projection);
}