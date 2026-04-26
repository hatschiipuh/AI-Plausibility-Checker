public sealed class LLMResultModel
{
    public double Score { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public string? NextInquiry { get; set; }
}