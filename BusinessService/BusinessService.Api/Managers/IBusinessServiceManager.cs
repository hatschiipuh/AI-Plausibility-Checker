using BusinessService.Api.Contracts;

namespace BusinessService.Api.Managers;

public interface IBusinessServiceManager
{
    Task<UserDto> GetOrCreateUserAsync(GetOrCreateUserRequest request, CancellationToken cancellationToken);
    Task<ValidationDto> CreateValidationAsync(CreateValidationRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ValidationListItemDto>> GetValidationsForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<ValidationDetailsDto> GetValidationDetailsAsync(Guid validationId, CancellationToken cancellationToken);
    Task<ValidateStepResponse> ValidateStepAsync(Guid validationId, ValidateStepRequest request, CancellationToken cancellationToken);
}
