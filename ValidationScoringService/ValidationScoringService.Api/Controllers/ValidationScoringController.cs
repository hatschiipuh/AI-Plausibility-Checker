using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ValidationScoringService.Api.Contracts;
using ValidationScoringService.Api.Infrastructure;
using ValidationScoringService.Api.Managers;

namespace ValidationScoringService.Api.Controllers;

[ApiController]
[ApiConventionType(typeof(PxDefaultApiConventions))]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/[controller]")]
[SystemTextJsonFormatter]
public sealed class ValidationScoringController : ControllerBase
{
    private readonly IValidationScoringManager manager;

    public ValidationScoringController(IValidationScoringManager manager)
    {
        this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    [HttpPost("score-step")]
    public Task<ActionResult<VSScoreStepResponse>> ScoreStepAsync([FromBody] VSScoreStepRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => manager.ScoreStepAsync(request, cancellationToken));
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
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
