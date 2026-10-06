using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpartanGym.Presentation.Serialization;

public static class JsonConventions
{
    public static void Apply(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
    }
}
