public class LLMValidationPrompt
{
    public LLMValidationPromptDescriptor  Descriptor { get; init; } = new LLMValidationPromptDescriptor();
    public LLMValidationPromptInput Input { get; init; } = new LLMValidationPromptInput();

    public  LLMValidationOutputDefinition OutputDefinition { get; init; } = new LLMValidationOutputDefinition();
}