using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ValidationScoringService.Api.Contracts;
using ValidationScoringService.Api.Managers;

namespace ValidationScoringService.Api.Controllers;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Route("api/[controller]")]
public sealed class ValidationScoringController : ControllerBase
{
    private readonly IValidationScoringManager manager;

    public ValidationScoringController(IValidationScoringManager manager)
    {
        this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    [HttpPost("score-step")]
    public Task<ActionResult<AiScoreStepResponse>> ScoreStepAsync([FromBody] AiScoreStepRequest request, CancellationToken cancellationToken)
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
