using ValidationScoringService.Api.Contracts;

namespace ValidationScoringService.Api.Tests.Llm;

public sealed class LLMResultModelValidatorTests
{
    [Fact]
    public void Validate_WithFinalResult_ReturnsResult()
    {
        var result = new LLMResultModel
        {
            Score = 0.8,
            Explanation = "The answer is sufficiently detailed.",
            IsFinal = true
        };

        var validatedResult = LLMResultModelValidator.Validate(result);

        Assert.Same(result, validatedResult);
    }

    [Fact]
    public void Validate_WithNonFinalResultAndNextInquiry_ReturnsResult()
    {
        var result = new LLMResultModel
        {
            Score = 0.5,
            Explanation = "More information is needed.",
            IsFinal = false,
            NextInquiry = "Can you provide more details?"
        };

        var validatedResult = LLMResultModelValidator.Validate(result);

        Assert.Same(result, validatedResult);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Validate_WithScoreOutsideAllowedRange_ThrowsInvalidOperationException(
        double score)
    {
        var result = new LLMResultModel
        {
            Score = score,
            Explanation = "Explanation",
            IsFinal = true
        };

        Assert.Throws<InvalidOperationException>(
            () => LLMResultModelValidator.Validate(result));
    }

    [Fact]
    public void Validate_WithNaNScore_ThrowsInvalidOperationException()
    {
        var result = new LLMResultModel
        {
            Score = double.NaN,
            Explanation = "Explanation",
            IsFinal = true
        };

        Assert.Throws<InvalidOperationException>(
            () => LLMResultModelValidator.Validate(result));
    }

    [Fact]
    public void Validate_WithoutExplanation_ThrowsInvalidOperationException()
    {
        var result = new LLMResultModel
        {
            Score = 0.5,
            IsFinal = true
        };

        Assert.Throws<InvalidOperationException>(
            () => LLMResultModelValidator.Validate(result));
    }

    [Fact]
    public void Validate_WithNonFinalResultWithoutNextInquiry_ThrowsInvalidOperationException()
    {
        var result = new LLMResultModel
        {
            Score = 0.5,
            Explanation = "More information is needed.",
            IsFinal = false
        };

        Assert.Throws<InvalidOperationException>(
            () => LLMResultModelValidator.Validate(result));
    }

    [Fact]
    public void Validate_WithNoRemainingFollowUpQuestions_AllowsMissingNextInquiry()
    {
        var result = new LLMResultModel
        {
            Score = 0.5,
            Explanation = "The answer has been assessed.",
            IsFinal = false
        };

        var validatedResult = LLMResultModelValidator.Validate(result, followUpQuestionAllowed: false);

        Assert.Same(result, validatedResult);
    }

    [Fact]
    public void Validate_WithOverlongNextInquiry_ThrowsInvalidOperationException()
    {
        var result = new LLMResultModel
        {
            Score = 0.5,
            Explanation = "More information is needed.",
            IsFinal = false,
            NextInquiry = new string('q', ValidationInputLimits.MaximumInquiryCharacters + 1)
        };

        Assert.Throws<InvalidOperationException>(
            () => LLMResultModelValidator.Validate(result));
    }

    [Fact]
    public void Validate_WithNullResult_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => LLMResultModelValidator.Validate(null));
    }
}
