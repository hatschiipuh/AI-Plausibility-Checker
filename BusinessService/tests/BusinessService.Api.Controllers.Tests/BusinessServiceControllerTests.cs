using BusinessService.Api.Contracts;
using BusinessService.Api.Controllers;
using BusinessService.Api.Managers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BusinessService.Api.Controllers.Tests;

public sealed class BusinessServiceControllerTests
{
    private readonly Mock<IBusinessServiceManager> managerMock = new();

    [Fact]
    public async Task CreateValidationAsync_ReturnsBadRequest_WhenArgumentExceptionThrown()
    {
        var userId = Guid.NewGuid();
        managerMock
            .Setup(m => m.CreateValidationAsync(It.IsAny<Guid>(), It.IsAny<CreateValidationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("invalid payload"));

        var controller = CreateController(userId);

        var result = await controller.CreateValidationAsync(new CreateValidationRequest { Question = "Q", InitialAnswer = "A" }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("invalid payload", badRequest.Value);
    }

    [Fact]
    public async Task GetValidationDetailsAsync_ReturnsNotFound_WhenManagerThrowsKeyNotFound()
    {
        var userId = Guid.NewGuid();
        managerMock
            .Setup(m => m.GetValidationDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        var controller = CreateController(userId);

        var result = await controller.GetValidationDetailsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task ValidateStepAsync_ReturnsBadRequest_WhenInvalidOperationThrown()
    {
        var userId = Guid.NewGuid();
        managerMock
            .Setup(m => m.ValidateStepAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<ValidateStepRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("already completed"));

        var controller = CreateController(userId);

        var result = await controller.ValidateStepAsync(Guid.NewGuid(), new ValidateStepRequest { Inquiry = "", UserAnswer = "" }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("already completed", badRequest.Value);
    }

    private BusinessServiceController CreateController(Guid userId)
    {
        var controller = new BusinessServiceController(managerMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                        "TestAuth"))
            }
        };

        return controller;
    }
}
