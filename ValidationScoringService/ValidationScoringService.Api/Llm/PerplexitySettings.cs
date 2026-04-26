public sealed class PerplexitySettings
{
    public string ApiKey { get; set; } = null!;
    public string BaseUrl { get; set; } = "https://api.perplexity.ai";
    public string Model  { get; set; } = "mistral-7b-instruct";
}