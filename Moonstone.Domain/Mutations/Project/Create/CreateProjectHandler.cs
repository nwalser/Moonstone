using Moonstone.Domain.Mutations.Project.ChangeName;
using Moonstone.Domain.Projection;
using Moonstone.Workspace.Stream;

namespace Moonstone.Domain.Mutations.Project.Create;

public class CreateProjectHandler : IMutationHandler<ProjectionModel, CreateProject>
{
    public void Handle(ProjectionModel model, CreateProject mutation)
    {
        model.CreatedProjects++;
    }
}