using BusinessService.Api.Contracts;

namespace BusinessService.Api.Managers;

public interface IBusinessServiceManager
{
    Task<User> GetOrCreateUserAsync(GetOrCreateUserRequest request, CancellationToken cancellationToken);
    Task<Validation> CreateValidationAsync(CreateValidationRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ValidationListItem>> GetValidationsForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<ValidationDetails> GetValidationDetailsAsync(Guid validationId, CancellationToken cancellationToken);
    Task<ValidateStepResponse> ValidateStepAsync(Guid validationId, ValidateStepRequest request, CancellationToken cancellationToken);
}
