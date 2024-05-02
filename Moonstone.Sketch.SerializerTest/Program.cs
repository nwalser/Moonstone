using Implementation;
using Implementation.Mutations;
using Implementation.Serializer;

var serializer = new MutationSerializer();

var data = serializer.Serialize(new CreateTask()
{
    Id = Guid.NewGuid(),
    Name = "Task 1"
});

var obj = serializer.Deserialize(data);


Console.ReadKey();