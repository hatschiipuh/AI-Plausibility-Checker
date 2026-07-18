using BusinessService.Api.Contracts;

namespace BusinessService.Api.Managers;

public interface IBusinessServiceManager
{
    Task<Validation> CreateValidationAsync(Guid currentUserId, CreateValidationRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ValidationListItem>> GetValidationsForCurrentUserAsync(Guid currentUserId, CancellationToken cancellationToken);
    Task<ValidationDetails> GetValidationDetailsAsync(Guid currentUserId, Guid validationId, CancellationToken cancellationToken);
    Task<ValidateStepResponse> ValidateStepAsync(Guid currentUserId, Guid validationId, ValidateStepRequest request, CancellationToken cancellationToken);
}
