using Moonstone.Workspace.Data;

namespace Moonstone.Workspace.Stream;

public class MutationHandler<TModel>
{
    public required Dictionary<Type, object> MutationHandlers { get; set; }
    
    public void Handle(TModel model, IMutation mutation)
    {
        if (MutationHandlers.TryGetValue(mutation.GetType(), out var handler))
        {
            var method = handler.GetType().GetMethod("Handle");

            if (method is null)
                throw new Exception("Message handler does not declare a correct Handle function");
            
            method.Invoke(handler, [model, mutation]);
        }
        else
        {
            throw new Exception("No matching command handler was found");
        }
    }
}