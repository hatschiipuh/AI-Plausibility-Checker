public class LLMValidationOutputDefinition
{
    public double Score {get; init;} = 0.0;
    public string Explanation { get; init; } = string.Empty;
    public bool IsFinal { get; init; }
    public string? NextInquiry { get; init; }
}