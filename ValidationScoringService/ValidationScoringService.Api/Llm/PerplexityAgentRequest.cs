using System.Text.Json.Serialization;

public sealed class PerplexityAgentRequest
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("instructions")]
    public string Instructions { get; init; } = string.Empty;

    [JsonPropertyName("input")]
    public PerplexityAgentInput[] Input { get; init; } = [];

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }

    [JsonPropertyName("response_format")]
    public PerplexityResponseFormat? ResponseFormat { get; init; }
}