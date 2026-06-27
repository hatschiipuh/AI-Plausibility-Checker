using ValidationScoringService.Api.Contracts;

public class LLMValidationPromptBuilder
{
    public static LLMValidationPrompt Build(VSScoreStepRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var prompt = new LLMValidationPrompt()
        {
            Input = new LLMValidationPromptInput()
            {
                Question = request.Question,
                InitialAnswer = request.InitialAnswer,
                Inquiries = request.Inquiries.Select(i => new LLMValidationPromptInquiry
                {
                    Inquiry = i.Inquiry,
                    UserAnswer = i.UserAnswer
                }).ToList()
            }

        };
        return prompt;
    }
}
        