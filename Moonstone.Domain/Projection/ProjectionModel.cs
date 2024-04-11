namespace Moonstone.Framework.Stream;

public class ProjectionModel
{
    public int CreatedProjects;
    
    

    public static ProjectionModel Empty()
    {
        return new ProjectionModel()
        {
            CreatedProjects = 0,
        };
    }
}