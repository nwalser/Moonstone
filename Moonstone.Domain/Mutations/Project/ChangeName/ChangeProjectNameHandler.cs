using Moonstone.Domain.Projection;
using Moonstone.Workspace.Stream;

namespace Moonstone.Domain.Mutations.Project.ChangeName;

public class ChangeProjectNameHandler : IMutationHandler<ProjectionModel, ChangeProjectName>
{
    public void Handle(ProjectionModel model, ChangeProjectName mutation)
    {
        model.CreatedProjects++;
    }
}