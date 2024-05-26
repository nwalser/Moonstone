using Opal;
using Opal.Domain;

var path = @"C:\Users\Nathaniel Walser\OneDrive - esp-engineering gmbh\Moonstone\workspace16";
var session = "session1";

var typeMap = new Dictionary<int, Type>()
{
    { 0, typeof(Project) },
    { 1, typeof(Todo) },
};

var database = new Database(typeMap, session, path);
database.Open();

var project = new Project()
{
    Id = Guid.NewGuid(),
    Name = "Project 1"
};

database.Update(project);
database.Update(project);
database.Update(project);

Console.WriteLine("Done");




