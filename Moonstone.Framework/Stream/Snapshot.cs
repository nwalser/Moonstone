
namespace Moonstone.Framework.Stream;

public class Snapshot<TModel> where TModel : new()
{
    public required DateTime LastMutationOccurence { get; set; }
    public required TModel Model { get; set; }
    
    
    public static Snapshot<TModel> Create()
    {
        return new Snapshot<TModel>()
        {
            Model = new TModel(),
            LastMutationOccurence = DateTime.MinValue,
        };
    }
    
    public void AppendMutation(Mutation mutation, MutationHandler<TModel> handler)
    {
        // todo implement collision of timestamps
        if (LastMutationOccurence > mutation.Occurence)
            throw new InvalidOperationException("Mutation is not applied in order");

        LastMutationOccurence = mutation.Occurence;

        handler.Handle(Model, mutation);
    }
}