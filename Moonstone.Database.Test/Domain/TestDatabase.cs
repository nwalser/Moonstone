namespace Moonstone.Database.Test.Domain;

public class TestDatabase : Database
{
    protected override Dictionary<int, Type> TypeMap { get; } = new()
    {
        { 0, typeof(Project) },
        { 1, typeof(Todo) }
    };
}