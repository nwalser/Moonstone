﻿using System.Text.Json;
using Newtonsoft.Json;

namespace DistributedSessions;

public static class NlJson
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        PreserveReferencesHandling = PreserveReferencesHandling.All,
        Formatting = Formatting.None
    };
    
    public static async Task Append(object obj, string file)
    {
        await using var stream = File.Open(file, FileMode.Append, FileAccess.Write, FileShare.Read);
        await using var sw = new StreamWriter(stream);

        var json = JsonConvert.SerializeObject(obj, Settings);
        await sw.WriteLineAsync(json);
    }

    public static async Task<List<TItem>> Read<TItem>(string file)
    {
        await using var stream = File.OpenRead(file);
        using var sr = new StreamReader(stream);

        var items = new List<TItem>();
        
        while (!sr.EndOfStream)
        {
            var line = await sr.ReadLineAsync();

            if (line is null) 
                break;

            var json = JsonConvert.DeserializeObject<TItem>(line, Settings);
            
            if(json is null) 
                continue;
            
            items.Add(json);
        }

        return items;
    }
}