using Amber.Domain.Documents.Project;
using Amber.Domain.Documents.Todo;
using Moonstone;

namespace Amber.Domain.Documents;

public class AmberWorkspace(string location) : Workspace(location, TypeMap)
{
    private static readonly Dictionary<int, Type> TypeMap = new()
    {
        {0, typeof(ProjectAggregate)},
        {1, typeof(TodoAggregate)},
    };
}