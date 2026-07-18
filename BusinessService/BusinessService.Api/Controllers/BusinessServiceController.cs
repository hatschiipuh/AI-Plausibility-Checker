using System.Net.Mime;
using BusinessService.Api.Contracts;
using BusinessService.Api.Managers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BusinessService.Api.Controllers;

[ApiController]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/[controller]")]
public sealed class BusinessServiceController : ControllerBase
{
    private readonly IBusinessServiceManager manager;

    public BusinessServiceController(IBusinessServiceManager manager)
    {
        this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    [HttpPost("Validations/CreateValidation")]
    public Task<ActionResult<Validation>> CreateValidationAsync([FromBody] CreateValidationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Task.FromResult<ActionResult<Validation>>(Unauthorized());
        }

        return ExecuteAsync(() => manager.CreateValidationAsync(currentUserId,request, cancellationToken));
    }

    [HttpGet("Validations/GetValidationsForUser")]
    public Task<ActionResult<IReadOnlyList<ValidationListItem>>> GetValidationsForCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Task.FromResult<ActionResult<IReadOnlyList<ValidationListItem>>>(Unauthorized());
        }

        return ExecuteAsync(() => manager.GetValidationsForCurrentUserAsync(currentUserId, cancellationToken));
    }

    [HttpGet("Validations/GetValidationDetails")]
    public Task<ActionResult<ValidationDetails>> GetValidationDetailsAsync([FromQuery] Guid validationId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Task.FromResult<ActionResult<ValidationDetails>>(Unauthorized());
        }

        return ExecuteAsync(() => manager.GetValidationDetailsAsync(currentUserId, validationId, cancellationToken));
    }

    [HttpPost("Validations/ValidateStep")]
    public Task<ActionResult<ValidateStepResponse>> ValidateStepAsync([FromQuery] Guid validationId, [FromBody] ValidateStepRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Task.FromResult<ActionResult<ValidateStepResponse>>(Unauthorized());
        }

        return ExecuteAsync(() => manager.ValidateStepAsync(currentUserId, validationId, request, cancellationToken));
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

    private bool TryGetCurrentUserId(out Guid userId)
    {
        userId = Guid.Empty;

        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claimValue, out userId);
    }
}
