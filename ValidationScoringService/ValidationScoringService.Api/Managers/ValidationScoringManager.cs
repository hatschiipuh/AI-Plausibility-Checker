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

        var modelResult = await llmClient.EvaluateStepAsync(request, cancellationToken).ConfigureAwait(false);

        return new VSScoreStepResponse
        {
            Score = modelResult.Score,
            AiExplanation = modelResult.Explanation,
            IsFinal = modelResult.IsFinal,
            NextInquiry = modelResult.NextInquiry
        };
    }
}
