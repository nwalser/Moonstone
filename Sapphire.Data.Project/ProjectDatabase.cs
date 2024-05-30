using Moonstone.Database;
using Sapphire.Data.Project.Entities;

namespace Sapphire.Data.Project;

public class ProjectDatabase : Database
{
    protected override Dictionary<int, Type> TypeMap { get; } = new()
    {
        { 0, typeof(Entities.Project) },
        { 1, typeof(Todo) },
        { 2, typeof(PossibleWorkerAssignment) },
    };
}