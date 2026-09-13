using System.Text.Json.Serialization;

public sealed class PerplexityAgentContent
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; init; }
}