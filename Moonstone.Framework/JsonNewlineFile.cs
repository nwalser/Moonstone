using Newtonsoft.Json;

namespace Moonstone.Framework;

public static class JsonNewlineFile
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        PreserveReferencesHandling = PreserveReferencesHandling.All,
        Formatting = Formatting.None
    };
    
    public static async Task AppendAsync(object obj, string file)
    {
        await using var stream = File.Open(file, FileMode.Append, FileAccess.Write, FileShare.Read);
        await using var sw = new StreamWriter(stream);

        var json = JsonConvert.SerializeObject(obj, Settings);
        await sw.WriteLineAsync(json);

        await sw.FlushAsync();
        sw.Close();
    }

    public static List<TItem> Read<TItem>(string file, CancellationToken ct = default)
    {
        using var stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(stream);

        var items = new List<TItem>();
        
        while (!sr.EndOfStream && !ct.IsCancellationRequested)
        {
            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
            var line = sr.ReadLine();

            if (line is null) 
                break;

            var json = JsonConvert.DeserializeObject<TItem>(line, Settings);
            
            if(json is null) 
                continue;
            
            items.Add(json);
        }

        stream.Close();
        
        return items;
    }
}