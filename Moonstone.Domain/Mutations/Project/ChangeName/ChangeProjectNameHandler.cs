﻿using Moonstone.Framework.Stream;

namespace Moonstone.Domain.Mutations.Project.ChangeName;

public class ChangeProjectNameHandler : IMutationHandler<ProjectionModel, ChangeProjectName>
{
    public void Handle(ProjectionModel model, ChangeProjectName mutation)
    {
        model.CreatedProjects++;
    }
}