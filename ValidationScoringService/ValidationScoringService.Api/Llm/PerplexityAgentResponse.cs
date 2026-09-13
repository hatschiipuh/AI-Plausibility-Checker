using System.Text.Json.Serialization;

public sealed class PerplexityAgentResponse
{
    [JsonPropertyName("output")]
    public IEnumerable<PerplexityAgentOutput>? Output { get; init; }
}
