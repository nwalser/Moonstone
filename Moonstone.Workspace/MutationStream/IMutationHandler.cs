namespace Moonstone.Workspace.MutationStream;

public interface IMutationHandler<in TProjection>
{
    public void Apply(TProjection projection);
}