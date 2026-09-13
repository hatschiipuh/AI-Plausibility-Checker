using System.Text.Json.Serialization;

public sealed class PerplexityAgentOutput
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public PerplexityAgentContent[] Content { get; init; } = [];
}