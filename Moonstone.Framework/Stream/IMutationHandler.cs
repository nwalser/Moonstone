namespace Moonstone.Framework.Stream;

public interface IMutationHandler<TModel, TMutation>
{
    public void Handle(TModel model, TMutation mutation);
}