
using System.Text.Json.Serialization;

public sealed class PerplexityResponseFormat
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "json_schema";

    [JsonPropertyName("json_schema")]
    public PerplexityJsonSchema JsonSchema { get; init; } = new();
}