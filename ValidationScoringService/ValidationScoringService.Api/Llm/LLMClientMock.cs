using System.Threading;
using System.Threading.Tasks;
using ValidationScoringService.Api.Contracts;

namespace ValidationScoringService.Api.Llm;

public interface ILLMClient
{
    Task<LLMResultModel> EvaluateStepAsync(VSScoreStepRequest request, CancellationToken cancellationToken);
}

public sealed class LLMClientMock : ILLMClient
{
    private int _callCount;

    public Task<LLMResultModel> EvaluateStepAsync(VSScoreStepRequest request, CancellationToken cancellationToken)
    {
        var currentCall = Interlocked.Increment(ref _callCount);
        var isFinal = currentCall >= 3;

        var result = new LLMResultModel
        {
            Score = isFinal ? 0.8 : 0.5,
            Explanation = $"Mock evaluation for validation {request.ValidationId}.",
            IsFinal = isFinal,
            NextInquiry = isFinal ? null : "Gibt es noch einen weiteren Aspekt, den du ergänzen möchtest?"
        };

        return Task.FromResult(result);
    }
}
