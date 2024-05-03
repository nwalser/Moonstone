using System.Diagnostics;
using Implementation.Mutations;
using Implementation.Serializer;
using Newtonsoft.Json;

var serializer = new MutationBinarySerializer();
var sw = new Stopwatch();

var obj = new CreateTask()
{
    Id = Guid.NewGuid(),
    Name = "Project 2"
};


sw.Restart();
for (var i = 0; i < 100_000; i++)
{
    JsonConvert.SerializeObject(obj);
}
Console.WriteLine("Json: " + sw.ElapsedMilliseconds);

sw.Restart();
for (var i = 0; i < 100_000; i++)
{
    serializer.Serialize(obj);
}
Console.WriteLine("Binary: " + sw.ElapsedMilliseconds);