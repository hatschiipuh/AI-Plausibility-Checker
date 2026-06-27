using System.ComponentModel.DataAnnotations;

public sealed class PerplexitySettings
{
    public static IReadOnlyCollection<string> SupportedModels { get; } =
        ["sonar"];

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://api.perplexity.ai";

    [Required]
    public string Model  { get; set; } = "sonar";

    public static bool IsSupportedModel(string? model)
    {
        return SupportedModels.Contains(model ?? string.Empty, StringComparer.OrdinalIgnoreCase);
    }
}
