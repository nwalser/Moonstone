using Amber.Domain.Documents.Project;
using Amber.Domain.Documents.Todo;
using Moonstone;

namespace Amber.Domain.Documents;

public class AmberWorkspace(string path) : Workspace(path, TypeMap)
{
    private static readonly Dictionary<int, Type> TypeMap = new()
    {
        {0, typeof(ProjectAggregate)},
        {1, typeof(TodoAggregate)},
    };
}