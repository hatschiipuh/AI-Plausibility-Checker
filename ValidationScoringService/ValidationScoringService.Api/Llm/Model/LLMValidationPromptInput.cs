public class LLMValidationPromptInput
{
    public string Question { get; init; } = string.Empty;
    public string InitialAnswer { get; init; } = string.Empty;
    public List<LLMValidationPromptInquiry> Inquiries { get; init; } = new List<LLMValidationPromptInquiry>();
}