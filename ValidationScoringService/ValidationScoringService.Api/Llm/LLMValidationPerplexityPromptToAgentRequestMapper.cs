
using System.Text.Json;

public static class LLMValidationPerplexityPromptToAgentRequestMapper
{
    public static PerplexityAgentRequest Map(
        string model,
        LLMValidationPerplexitySonarPrompt prompt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model);
        ArgumentNullException.ThrowIfNull(prompt);

        var instructions = string.Join(
            Environment.NewLine + Environment.NewLine,
            new[] { LLMValidationPerplexitySonarPrompt.SYSTEMTASK }
                .Concat(LLMValidationPerplexitySonarPrompt.HINTS));

        return new PerplexityAgentRequest
        {
            Model = model,
            Instructions = instructions,
            Stream = false,
            ResponseFormat = PerplexityResponseFormatFactory.CreateValidationResult(),
            Input =
            [
                new PerplexityAgentInput
                {
                    Type = "message",
                    Role = LLMValidationPerplexitySonarPrompt.USERROLE,
                    Content = JsonSerializer.Serialize(prompt.ValidationPrompt)
                }
            ]
        };
    }
}