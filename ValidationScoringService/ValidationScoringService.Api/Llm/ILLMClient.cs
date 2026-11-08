using System.Threading;
using System.Threading.Tasks;
using ValidationScoringService.Api.Contracts;

namespace ValidationScoringService.Api.Llm;

public interface ILLMClient
{
    Task<LLMResultModel> EvaluateStepAsync(VSScoreStepRequest request, CancellationToken cancellationToken);
}
