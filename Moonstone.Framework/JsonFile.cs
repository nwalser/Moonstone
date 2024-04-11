using Newtonsoft.Json;

namespace Moonstone.Framework;

public class JsonFile
{
    private static readonly JsonSerializer Serializer = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        PreserveReferencesHandling = PreserveReferencesHandling.All,
        Formatting = Formatting.None
    };
    
    public static void Serialize(object value, System.IO.Stream s)
    {
        using var writer = new StreamWriter(s);
        using var jsonWriter = new JsonTextWriter(writer);
        
        Serializer.Serialize(jsonWriter, value);
        jsonWriter.Flush();
    }

    public static T Deserialize<T>(System.IO.Stream s)
    {
        using var reader = new StreamReader(s);
        using var jsonReader = new JsonTextReader(reader);
        
        return Serializer.Deserialize<T>(jsonReader);
    }
}