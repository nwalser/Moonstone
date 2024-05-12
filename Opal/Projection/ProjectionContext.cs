using Microsoft.EntityFrameworkCore;

namespace Opal.Projection;

public class ProjectionContext(DbContextOptions<ProjectionContext> options) : DbContext(options)
{
    
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        
    }
}