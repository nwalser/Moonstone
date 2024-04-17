
namespace Moonstone.Framework.Stream;

public class Snapshot<TModel> where TModel : new()
{
    public required Guid LastMutationId { get; set; }
    public required TModel Model { get; set; }
    
    
    public required byte[] Data { get; set; }
    
    public static Snapshot<TModel> Create()
    {
        return new Snapshot<TModel>()
        {
            Model = new TModel(),
            LastMutationId = Guid.Empty,
            Data = Enumerable.Range(0, 1024 * 1024 * 512).Select(i => (byte)(i%255)).ToArray()
        };
    }
    
    public void AppendMutation(Mutation mutation, MutationHandler<TModel> handler)
    {
        if (LastMutationId >= mutation.Id)
            throw new InvalidOperationException("Mutation is not applied in order");

        LastMutationId = mutation.Id;

        handler.Handle(Model, mutation);
    }
}