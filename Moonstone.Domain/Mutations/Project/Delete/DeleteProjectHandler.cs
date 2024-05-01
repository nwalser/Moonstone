using Moonstone.Domain.Mutations.Project.ChangeName;
using Moonstone.Domain.Projection;
using Moonstone.Workspace.Stream;

namespace Moonstone.Domain.Mutations.Project.Delete;

public class DeleteProjectHandler : IMutationHandler<ProjectionModel, DeleteProject>
{
    public void Handle(ProjectionModel model, DeleteProject mutation)
    {
        model.CreatedProjects--;
    }
}