using System.Text.Json;

namespace Moonstone;

public record Entry
{
    public required long Occurence { get; init; }
    public required int TypeId { get; init; }
    public required object Mutation { get; init; }

    public void Serialize(Stream stream)
    {
        using var sw = new StreamWriter(stream);

        {
            var typeId = TypeId.ToString();
            sw.Write(typeId);
            sw.Write(",");
        }

        {
            var bytes = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
            var base64 = Convert.ToBase64String(bytes);
            sw.Write(base64);
            sw.Write(",");
        }

        {
            var json = JsonSerializer.Serialize(Mutation);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);
            sw.Write(base64);
            sw.WriteLine();
        }
    }
}