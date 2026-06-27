using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ValidationScoringService.Api.Contracts;
using ValidationScoringService.Api.Controllers;
using ValidationScoringService.Api.Managers;
using ValidationScoringService.Api.Tests.TestData;

namespace ValidationScoringService.Api.Tests.Controllers;

public sealed class ValidationScoringControllerTests
{
    [Fact]
    public async Task ScoreStepAsync_WhenManagerSucceeds_ReturnsOkObjectResult()
    {
        var request = AiScoreStepRequestBuilder.CreateValidRequest();
        var response = new VSScoreStepResponse
        {
            Score = 0.91,
            AiExplanation = "Looks good",
            IsFinal = false,
            NextInquiry = "Anything to add?"
        };

        var managerMock = new Mock<IValidationScoringManager>(MockBehavior.Strict);
        managerMock
            .Setup(manager => manager.ScoreStepAsync(request, CancellationToken.None))
            .ReturnsAsync(response);

        var controller = new ValidationScoringController(managerMock.Object);

        var actionResult = await controller.ScoreStepAsync(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(response, okResult.Value);
        managerMock.VerifyAll();
    }

    public static IEnumerable<object[]> BadRequestExceptionBuilders()
    {
        yield return new object[]
        {
            new Func<Exception>(() => new ArgumentException("Invalid request"))
        };
    }

    [Theory]
    [MemberData(nameof(BadRequestExceptionBuilders))]
    public async Task ScoreStepAsync_WhenManagerThrowsBadRequestException_ReturnsBadRequest(Func<Exception> exceptionFactory)
    {
        var request = AiScoreStepRequestBuilder.CreateValidRequest();
        var exception = exceptionFactory();
        var managerMock = new Mock<IValidationScoringManager>(MockBehavior.Strict);
        managerMock
            .Setup(manager => manager.ScoreStepAsync(request, CancellationToken.None))
            .ThrowsAsync(exception);

        var controller = new ValidationScoringController(managerMock.Object);

        var actionResult = await controller.ScoreStepAsync(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal(exception.Message, badRequest.Value);
        managerMock.VerifyAll();
    }

    [Fact]
    public async Task ScoreStepAsync_WhenManagerThrowsInvalidOperationException_ReturnsInternalServerError()
    {
        var request = AiScoreStepRequestBuilder.CreateValidRequest();
        var exception = new InvalidOperationException("Currently unavailable");
        var managerMock = new Mock<IValidationScoringManager>(MockBehavior.Strict);
        managerMock
            .Setup(manager => manager.ScoreStepAsync(request, CancellationToken.None))
            .ThrowsAsync(exception);

        var controller = new ValidationScoringController(managerMock.Object);

        var actionResult = await controller.ScoreStepAsync(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(exception.Message, objectResult.Value);
        managerMock.VerifyAll();
    }

    [Fact]
    public async Task ScoreStepAsync_WhenManagerThrowsHttpRequestException_ReturnsBadGateway()
    {
        var request = AiScoreStepRequestBuilder.CreateValidRequest();
        var exception = new HttpRequestException("Provider failed");
        var managerMock = new Mock<IValidationScoringManager>(MockBehavior.Strict);
        managerMock
            .Setup(manager => manager.ScoreStepAsync(request, CancellationToken.None))
            .ThrowsAsync(exception);

        var controller = new ValidationScoringController(managerMock.Object);

        var actionResult = await controller.ScoreStepAsync(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status502BadGateway, objectResult.StatusCode);
        Assert.Equal(exception.Message, objectResult.Value);
        managerMock.VerifyAll();
    }

    [Fact]
    public async Task ScoreStepAsync_WhenManagerThrowsKeyNotFound_ReturnsNotFound()
    {
        var request = AiScoreStepRequestBuilder.CreateValidRequest();
        var managerMock = new Mock<IValidationScoringManager>(MockBehavior.Strict);
        managerMock
            .Setup(manager => manager.ScoreStepAsync(request, CancellationToken.None))
            .ThrowsAsync(new KeyNotFoundException());

        var controller = new ValidationScoringController(managerMock.Object);

        var actionResult = await controller.ScoreStepAsync(request, CancellationToken.None);

        Assert.IsType<NotFoundResult>(actionResult.Result);
        managerMock.VerifyAll();
    }
}
