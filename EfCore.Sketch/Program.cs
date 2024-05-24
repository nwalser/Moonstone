using EfCore.Sketch;
using EfCore.Sketch.Sync;
using Microsoft.EntityFrameworkCore;

var optionsBuilder = new DbContextOptionsBuilder<SyncContext>();
optionsBuilder.UseSqlite("Data Source=./data.db");

var context = new OpalContext(optionsBuilder.Options);

await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

var project = new Project()
{
    Id = Guid.NewGuid(),
    Name = "Project 1"
};

context.Projects.Add(project);

await context.SaveChangesAsync();

project.Name = "Project 2";
await context.SaveChangesAsync();

var todo = new Todo()
{
    Id = Guid.NewGuid(),
    Name = "Do this instead of this,",
    Project = project
};

context.Add(todo);
await context.SaveChangesAsync();


var project3 = new Project()
{
    Id = Guid.NewGuid(),
    Name = "Project 3"
};


todo.Project = project3;
context.Add(project3);
await context.SaveChangesAsync();

context.Remove(project);
context.Remove(project3);
await context.SaveChangesAsync();

Console.WriteLine("Done");