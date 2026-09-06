using System.Text.Json.Serialization;

public sealed class PerplexityAgentResponse
{
    [JsonPropertyName("output_text")]
    public string? OutputText { get; init; }
}