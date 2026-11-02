using System;
using System.Threading;
using System.Threading.Tasks;
using ValidationScoringService.Api.Contracts;
using ValidationScoringService.Api.Llm;

namespace ValidationScoringService.Api.Managers;

public sealed class ValidationScoringManager : IValidationScoringManager
{
    private readonly ILLMClient llmClient;

    public ValidationScoringManager(ILLMClient llmClient)
    {
        this.llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
    }

    public async Task<VSScoreStepResponse> ScoreStepAsync(VSScoreStepRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ValidationId == Guid.Empty)
        {
            throw new ArgumentException("ValidationId must not be empty.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Question))
        {
            throw new ArgumentException("Question must not be empty.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.InitialAnswer))
        {
            throw new ArgumentException("InitialAnswer must not be empty.", nameof(request));
        }

        if (request.Question.Length > ValidationInputLimits.MaximumQuestionCharacters)
        {
            throw new ArgumentException(
                $"Question cannot exceed {ValidationInputLimits.MaximumQuestionCharacters} characters.",
                nameof(request));
        }

        if (request.InitialAnswer.Length > ValidationInputLimits.MaximumInitialAnswerCharacters)
        {
            throw new ArgumentException(
                $"InitialAnswer cannot exceed {ValidationInputLimits.MaximumInitialAnswerCharacters} characters.",
                nameof(request));
        }

        if (request.Inquiries is null)
        {
            throw new ArgumentException("Inquiries must not be null.", nameof(request));
        }

        if (request.Inquiries.Count > ValidationInputLimits.MaximumFollowUpQuestions)
        {
            throw new ArgumentException(
                $"A validation cannot contain more than {ValidationInputLimits.MaximumFollowUpQuestions} follow-up questions.",
                nameof(request));
        }

        long totalPromptInputCharacters = request.Question.Length + request.InitialAnswer.Length;

        foreach (var inquiry in request.Inquiries)
        {
            if (inquiry is null)
            {
                throw new ArgumentException("Inquiries must not contain null items.", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(inquiry.Inquiry) || string.IsNullOrWhiteSpace(inquiry.UserAnswer))
            {
                throw new ArgumentException("Every follow-up inquiry and answer is required.", nameof(request));
            }

            if (inquiry.Inquiry.Length > ValidationInputLimits.MaximumInquiryCharacters)
            {
                throw new ArgumentException(
                    $"Inquiry cannot exceed {ValidationInputLimits.MaximumInquiryCharacters} characters.",
                    nameof(request));
            }

            if (inquiry.UserAnswer.Length > ValidationInputLimits.MaximumUserAnswerCharacters)
            {
                throw new ArgumentException(
                    $"UserAnswer cannot exceed {ValidationInputLimits.MaximumUserAnswerCharacters} characters.",
                    nameof(request));
            }

            totalPromptInputCharacters += inquiry.Inquiry.Length + (long)inquiry.UserAnswer.Length;
        }

        if (totalPromptInputCharacters > ValidationInputLimits.MaximumPromptInputCharacters)
        {
            throw new ArgumentException(
                $"Prompt input cannot exceed {ValidationInputLimits.MaximumPromptInputCharacters} characters.",
                nameof(request));
        }

        var modelResult = await llmClient.EvaluateStepAsync(request, cancellationToken).ConfigureAwait(false);
        var followUpQuestionLimitReached =
            request.Inquiries.Count == ValidationInputLimits.MaximumFollowUpQuestions;
        var isFinal = modelResult.IsFinal || followUpQuestionLimitReached;

        return new VSScoreStepResponse
        {
            Score = modelResult.Score,
            AiExplanation = modelResult.Explanation,
            IsFinal = isFinal,
            NextInquiry = isFinal ? null : modelResult.NextInquiry
        };
    }
}
