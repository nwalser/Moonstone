namespace Opal.Cache;

public class Mutation
{
    public required Guid Id { get; init; }
    public required byte[] Data { get; init; }
    
    public required bool Projected { get; set; }
}