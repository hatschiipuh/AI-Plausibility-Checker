using ValidationScoringService.Api.Contracts;
using ValidationScoringService.Api.Tests.TestData;

namespace ValidationScoringService.Api.Tests.Llm;

public sealed class LLMValidationPromptBuilderTests
{
    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 9)]
    [InlineData(10, 0)]
    public void Build_IncludesRemainingFollowUpQuestionBudget(int askedQuestions, int remainingQuestions)
    {
        var request = AiScoreStepRequestBuilder.CreateValidRequest();
        request.Inquiries =
        [
            .. Enumerable.Range(0, askedQuestions)
                .Select(index => new VSScoreStepRequestInquiry
                {
                    Inquiry = $"Follow-up question {index + 1}",
                    UserAnswer = "Follow-up answer"
                })
        ];

        var prompt = LLMValidationPromptBuilder.Build(request);

        Assert.Equal(ValidationInputLimits.MaximumFollowUpQuestions, prompt.Input.MaximumFollowUpQuestions);
        Assert.Equal(askedQuestions, prompt.Input.FollowUpQuestionsAsked);
        Assert.Equal(remainingQuestions, prompt.Input.RemainingFollowUpQuestions);
    }
}
