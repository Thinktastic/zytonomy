
namespace Zytonomy.Api.Support;

/// <summary>
/// Implement a custom serializer to support usage of the System.Text.Json serializer.
/// </summary>
public class SystemTextJsonCosmosSerializer : CosmosSerializer
{
    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions {
        PropertyNameCaseInsensitive = true,
        IncludeFields = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public override T FromStream<T>(Stream stream)
    {
        using(BinaryReader reader = new BinaryReader(stream)) {
            return JsonSerializer.Deserialize<T>(
                reader.ReadBytes(Convert.ToInt32(stream.Length)),
                _serializerOptions);
        }
    }

    public override Stream ToStream<T>(T input)
    {
        string json = JsonSerializer.Serialize<T>(input, _serializerOptions);

        return new MemoryStream(Encoding.UTF8.GetBytes(json));
    }
}

