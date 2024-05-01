namespace Moonstone.Workspace.Stream;

public interface IMutationHandler<TModel, TMutation>
{
    public void Handle(TModel model, TMutation mutation);
}