using ValidationScoringService.Api.Contracts;

public static class LLMResultModelValidator
{
    public static LLMResultModel Validate(
        LLMResultModel? result,
        bool followUpQuestionAllowed = true)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (double.IsNaN(result.Score)
            || double.IsInfinity(result.Score)
            || result.Score < 0
            || result.Score > 1)
        {
            throw new InvalidOperationException(
                "The LLM response contains an invalid score.");
        }

        if (string.IsNullOrWhiteSpace(result.Explanation))
        {
            throw new InvalidOperationException(
                "The LLM response contains no explanation.");
        }

        if (result.NextInquiry?.Length > ValidationInputLimits.MaximumInquiryCharacters)
        {
            throw new InvalidOperationException(
                $"The LLM response contains a follow-up question longer than {ValidationInputLimits.MaximumInquiryCharacters} characters.");
        }

        if (!result.IsFinal
            && followUpQuestionAllowed
            && string.IsNullOrWhiteSpace(result.NextInquiry))
        {
            throw new InvalidOperationException(
                "A non-final LLM response must contain a next inquiry.");
        }

        return result;
    }
}