using System.Net.Mime;
using BusinessService.Api.Contracts;
using BusinessService.Api.Managers;
using Microsoft.AspNetCore.Mvc;

namespace BusinessService.Api.Controllers;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Route("api")]
[ApiExplorerSettings(GroupName = "Nswag")]
public sealed class BusinessServiceController : ControllerBase
{
    private readonly IBusinessServiceManager manager;

    public BusinessServiceController(IBusinessServiceManager manager)
    {
        this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    [HttpPost("users/get-or-create")]
    public Task<ActionResult<UserDto>> GetOrCreateUserAsync([FromBody] GetOrCreateUserRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => manager.GetOrCreateUserAsync(request, cancellationToken));
    }

    [HttpPost("validations")]
    public Task<ActionResult<ValidationDto>> CreateValidationAsync([FromBody] CreateValidationRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => manager.CreateValidationAsync(request, cancellationToken));
    }

    [HttpGet("validations")]
    public Task<ActionResult<IReadOnlyList<ValidationListItemDto>>> GetValidationsForUserAsync([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => manager.GetValidationsForUserAsync(userId, cancellationToken));
    }

    [HttpGet("validations/{validationId:guid}")]
    public Task<ActionResult<ValidationDetailsDto>> GetValidationDetailsAsync([FromRoute] Guid validationId, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => manager.GetValidationDetailsAsync(validationId, cancellationToken));
    }

    [HttpPost("validations/{validationId:guid}/validate")]
    public Task<ActionResult<ValidateStepResponse>> ValidateStepAsync([FromRoute] Guid validationId, [FromBody] ValidateStepRequest request, CancellationToken cancellationToken)
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
