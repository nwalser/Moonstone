using Moonstone.Workspace.Data;

namespace Moonstone.Workspace.Stream;

public class Snapshot<TModel> where TModel : new()
{
    public required Guid LastMutationId { get; set; }
    public required TModel Model { get; set; }
    
    
    public static Snapshot<TModel> Create()
    {
        return new Snapshot<TModel>()
        {
            Model = new TModel(),
            LastMutationId = Guid.Empty,
        };
    }
    
    public void AppendMutation(MutationEnvelope mutation, MutationHandler<TModel> handler)
    {
        if (LastMutationId >= mutation.Id)
            throw new InvalidOperationException("Mutation is not applied in order");

        LastMutationId = mutation.Id;

        handler.Handle(Model, mutation.Mutation);
    }
}