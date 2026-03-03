using System.Threading;
using System.Threading.Tasks;
using ValidationScoringService.Api.Contracts;

namespace ValidationScoringService.Api.Llm;

public interface ILLMClient
{
    Task<AiModelResult> EvaluateStepAsync(AiScoreStepRequest request, CancellationToken cancellationToken);
}

public sealed class AiModelResult
{
    public double Score { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public string? NextInquiry { get; set; }
}

public sealed class LLMClientMock : ILLMClient
{
    public Task<AiModelResult> EvaluateStepAsync(AiScoreStepRequest request, CancellationToken cancellationToken)
    {
        var result = new AiModelResult
        {
            Score = 0.5,
            Explanation = $"Mock evaluation for validation {request.ValidationId}.",
            IsFinal = false,
            NextInquiry = "Gibt es noch einen weiteren Aspekt, den du ergänzen möchtest?"
        };

        return Task.FromResult(result);
    }
}
