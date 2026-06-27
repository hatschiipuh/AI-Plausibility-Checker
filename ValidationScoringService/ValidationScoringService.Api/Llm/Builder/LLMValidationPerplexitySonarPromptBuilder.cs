using ValidationScoringService.Api.Contracts;

public class LLMValidationPerplexitySonarPromptBuilder
{
    public static LLMValidationPerplexitySonarPrompt Build(VSScoreStepRequest request)
    {
        var prompt = new LLMValidationPerplexitySonarPrompt
        {
            ValidationPrompt = LLMValidationPromptBuilder.Build(request),
        };

        return prompt;
    }
}

