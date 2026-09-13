using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class PerplexityJsonSchema
{
    [JsonPropertyName("name")]
    public string SchemaIdentifier { get; init; } = "ValidationResult";

    [JsonPropertyName("schema")]
    public JsonElement Schema { get; init; }
}