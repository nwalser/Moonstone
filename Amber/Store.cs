using System.ComponentModel.DataAnnotations;
using ProtoBuf;

namespace Amber;

public class Store
{
    private const PrefixStyle PrefixStyle = ProtoBuf.PrefixStyle.Base128;
    private const int FieldNumber = 0;
    
    private readonly string _path;
    private readonly Dictionary<int, Type> _documentTypes;
    
    public Store(string path, Dictionary<int, Type> documentTypes)
    {
        _path = path;
        _documentTypes = documentTypes;
    }

    public async Task Initialize()
    {
        var documentDirectories = Directory.GetDirectories(GetDocumentsFolder());

        foreach (var documentDirectory in documentDirectories)
        {
            var segments = documentDirectory.Split("_");
            var documentId = Guid.Parse(segments[0]);
            var documentTypeId = Convert.ToInt32(segments[1]); 
            var documentType = _documentTypes[documentTypeId]; 
            
            // read all mutations for given document
            var streamsFolder = GetStreamsFolder(documentId);
            var mutationFiles = Directory.EnumerateFiles(streamsFolder, ".bin");
            var mutationEnvelopes = new List<MutationEnvelope>();
            foreach (var mutationFile in mutationFiles)
            {
                await using var stream = File.Open(mutationFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                while (stream.Position < stream.Length)
                {
                    var mutationEnvelope = Serializer.DeserializeWithLengthPrefix<MutationEnvelope>(stream, PrefixStyle, FieldNumber);
                    mutationEnvelopes.Add(mutationEnvelope);
                }
            }
            
            // apply all mutations to given document
            
        }
    }

    private string GetDocumentsFolder() => Path.Join(_path, "documents");
    private string GetStreamsFolder(Guid documentId) => Path.Join(GetDocumentsFolder(), documentId.ToString(), "streams");
}