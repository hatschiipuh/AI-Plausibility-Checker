using System.Net.Mime;
using System.Security.Claims;
using BusinessService.Api.Contracts;
using BusinessService.Api.Infrastructure;
using BusinessService.Api.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessService.Api.Controllers;

[ApiController]
[ApiConventionType(typeof(PxDefaultApiConventions))]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/[controller]")]
[SystemTextJsonFormatter]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthManager manager;

    public AuthController(IAuthManager manager)
    {
        this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public Task<ActionResult<AuthResponse>> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => manager.RegisterAsync(request, cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public Task<ActionResult<AuthResponse>> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => manager.LoginAsync(request, cancellationToken));
    }

    [Authorize]
    [HttpGet("me")]
    public Task<ActionResult<User>> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Task.FromResult<ActionResult<User>>(Unauthorized());
        }

        return ExecuteAsync(() => manager.GetCurrentUserAsync(currentUserId, cancellationToken));
    }

    [Authorize]
    [HttpDelete("me")]
    public async Task<ActionResult> DeleteCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return Unauthorized();
        }

        try
        {
            await manager.DeleteCurrentUserAsync(currentUserId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
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
