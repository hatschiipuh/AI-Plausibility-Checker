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
                MaximumFollowUpQuestions = ValidationInputLimits.MaximumFollowUpQuestions,
                FollowUpQuestionsAsked = request.Inquiries.Count,
                RemainingFollowUpQuestions =
                    ValidationInputLimits.MaximumFollowUpQuestions - request.Inquiries.Count,
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
        