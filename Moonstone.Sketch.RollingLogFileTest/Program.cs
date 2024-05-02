using System.Diagnostics;
using Abstractions.Log;
using Abstractions.Mutation;
using Abstractions.Serializer;
using Implementation.Mutations;
using Implementation.Serializer;

var folder = "./test/";
Directory.CreateDirectory(folder);

var byteTextSerializer = new ByteTextSerializer();
var mutationBinarySerializer = new MutationBinarySerializer();
var mutationEnvelopeSerializer = new MutationEnvelopeSerializer<IMutation>(mutationBinarySerializer, byteTextSerializer);

var rollingLogFile = new RollingLog<MutationEnvelope<IMutation>>(folder, mutationEnvelopeSerializer);

rollingLogFile.Initialize();

var sw = Stopwatch.StartNew();

for (var i = 0; i < 100_000; i++)
{
    rollingLogFile.Append(new MutationEnvelope<IMutation>()
    {
        Id = Guid.NewGuid(),
        Mutation = new CreateTask()
        {
            Id = Guid.NewGuid(),
            Name = "Task 1"
        }
    });
}
Console.WriteLine(sw.ElapsedMilliseconds);

sw.Restart();

var file = "./test/0.bin";

var logFile = new LogFile<MutationEnvelope<IMutation>>(mutationEnvelopeSerializer);
var items = logFile.Read(file).ToList();

Console.WriteLine(sw.ElapsedMilliseconds);
