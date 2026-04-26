using System.Threading;
using System.Threading.Tasks;
using ValidationScoringService.Api.Contracts;

namespace ValidationScoringService.Api.Managers;

public interface IValidationScoringManager
{
    Task<VSScoreStepResponse> ScoreStepAsync(VSScoreStepRequest request, CancellationToken cancellationToken);
}
