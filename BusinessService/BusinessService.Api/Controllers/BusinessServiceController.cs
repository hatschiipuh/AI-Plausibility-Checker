using System.Net.Mime;
using BusinessService.Api.Contracts;
using BusinessService.Api.Infrastructure;
using BusinessService.Api.Managers;
using Microsoft.AspNetCore.Mvc;

namespace BusinessService.Api.Controllers;

[ApiController]
[ApiConventionType(typeof(PxDefaultApiConventions))]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/[controller]")]
[SystemTextJsonFormatter]
public sealed class BusinessServiceController : ControllerBase
{
    private readonly IBusinessServiceManager manager;

    public BusinessServiceController(IBusinessServiceManager manager)
    {
        this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    [HttpPost("users/GetOrCreateUser")]
    public Task<ActionResult<UserDto>> GetOrCreateUserAsync([FromBody] GetOrCreateUserRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => manager.GetOrCreateUserAsync(request, cancellationToken));
    }

    [HttpPost("Validations/CreateValidation")]
    public Task<ActionResult<ValidationDto>> CreateValidationAsync([FromBody] CreateValidationRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => manager.CreateValidationAsync(request, cancellationToken));
    }

    [HttpGet("Validations/GetValidationsForUser")]
    public Task<ActionResult<IReadOnlyList<ValidationListItemDto>>> GetValidationsForUserAsync([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => manager.GetValidationsForUserAsync(userId, cancellationToken));
    }

    [HttpGet("Validations/GetValidationDetails")]
    public Task<ActionResult<ValidationDetailsDto>> GetValidationDetailsAsync([FromQuery] Guid validationId, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => manager.GetValidationDetailsAsync(validationId, cancellationToken));
    }

    [HttpPost("Validations/ValidateStep")]
    public Task<ActionResult<ValidateStepResponse>> ValidateStepAsync([FromQuery] Guid validationId, [FromBody] ValidateStepRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => manager.ValidateStepAsync(validationId, request, cancellationToken));
    }

    private async Task<ActionResult<T>> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var result = await action().ConfigureAwait(false);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
