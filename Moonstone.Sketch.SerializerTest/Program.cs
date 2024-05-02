using Abstractions;
using Implementation.Mutations;
using Implementation.Serializer;

var mutationBinarySerializer = new MutationBinarySerializer();
var byteTextSerializer = new ByteTextSerializer();
var mutationEnvelopeSerializer = new MutationEnvelopeSerializer<IMutation>(mutationBinarySerializer, byteTextSerializer);

var str = mutationEnvelopeSerializer.Serialize(new MutationEnvelope<IMutation>()
{
    Id = Guid.NewGuid(),
    Mutation = new CreateTask()
    {
        Id = Guid.NewGuid(),
        Name = "Project 2"
    }
});

var obj = mutationEnvelopeSerializer.Deserialize(str);


var str2 = mutationEnvelopeSerializer.Serialize(new MutationEnvelope<IMutation>()
{
    Id = Guid.NewGuid(),
    Mutation = new DeleteTask()
    {
        Id = Guid.NewGuid(),
    }
});

var obj2 = mutationEnvelopeSerializer.Deserialize(str2);

Console.ReadKey();