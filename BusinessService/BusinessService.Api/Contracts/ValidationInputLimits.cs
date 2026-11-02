namespace BusinessService.Api.Contracts;

public static class ValidationInputLimits
{
    public const int MaximumQuestionCharacters = 2_000;
    public const int MaximumInitialAnswerCharacters = 8_000;
    public const int MaximumInquiryCharacters = 2_000;
    public const int MaximumUserAnswerCharacters = 8_000;
    public const int MaximumFollowUpQuestions = 10;
    public const int MaximumPromptInputCharacters = 40_000;
}
