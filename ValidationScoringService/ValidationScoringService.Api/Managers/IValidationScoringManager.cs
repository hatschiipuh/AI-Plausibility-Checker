using System.Threading;
using System.Threading.Tasks;
using ValidationScoringService.Api.Contracts;

namespace ValidationScoringService.Api.Managers;

public interface IValidationScoringManager
{
    Task<AiScoreStepResponse> ScoreStepAsync(AiScoreStepRequest request, CancellationToken cancellationToken);
}
