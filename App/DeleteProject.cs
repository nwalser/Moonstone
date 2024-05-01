using Framework;
using MessagePack;

namespace App;

[MessagePackObject]
public class DeleteProject : IMutation, IMutationHandler<Projection>
{
    [Key(0)] public required Guid Id { get; set; }
    [Key(1)] public required string Name { get; set; }

    public void Apply(Projection projection)
    {
        projection.Counter--;
    }
}