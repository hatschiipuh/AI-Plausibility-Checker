using BusinessService.Api.Contracts;
using BusinessService.Api.Controllers;
using BusinessService.Api.Managers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BusinessService.Api.Controllers.Tests;

public sealed class BusinessServiceControllerTests
{
    private readonly Mock<IBusinessServiceManager> managerMock = new();

    [Fact]
    public async Task GetOrCreateUserAsync_ReturnsOk_WithUser()
    {
        var expected = new User { Id = Guid.NewGuid(), Email = "user@example.com", DisplayName = "User" };
        managerMock
            .Setup(m => m.GetOrCreateUserAsync(It.IsAny<GetOrCreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new BusinessServiceController(managerMock.Object);

        var result = await controller.GetOrCreateUserAsync(new GetOrCreateUserRequest { Email = "user@example.com", DisplayName = "User" }, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, okResult.Value);
    }

    [Fact]
    public async Task CreateValidationAsync_ReturnsBadRequest_WhenArgumentExceptionThrown()
    {
        managerMock
            .Setup(m => m.CreateValidationAsync(It.IsAny<CreateValidationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("invalid payload"));

        var controller = new BusinessServiceController(managerMock.Object);

        var result = await controller.CreateValidationAsync(new CreateValidationRequest { UserId = Guid.NewGuid(), Question = "Q", InitialAnswer = "A" }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("invalid payload", badRequest.Value);
    }

    [Fact]
    public async Task GetValidationDetailsAsync_ReturnsNotFound_WhenManagerThrowsKeyNotFound()
    {
        managerMock
            .Setup(m => m.GetValidationDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        var controller = new BusinessServiceController(managerMock.Object);

        var result = await controller.GetValidationDetailsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task ValidateStepAsync_ReturnsBadRequest_WhenInvalidOperationThrown()
    {
        managerMock
            .Setup(m => m.ValidateStepAsync(It.IsAny<Guid>(), It.IsAny<ValidateStepRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("already completed"));

        var controller = new BusinessServiceController(managerMock.Object);

        var result = await controller.ValidateStepAsync(Guid.NewGuid(), new ValidateStepRequest { Inquiry = "", UserAnswer = "" }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("already completed", badRequest.Value);
    }
}
