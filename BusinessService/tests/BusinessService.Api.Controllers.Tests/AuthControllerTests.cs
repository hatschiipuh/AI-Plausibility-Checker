using BusinessService.Api.Contracts;
using BusinessService.Api.Controllers;
using BusinessService.Api.Managers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BusinessService.Api.Controllers.Tests;

public sealed class AuthControllerTests
{
    private readonly Mock<IAuthManager> managerMock = new();

    [Fact]
    public async Task RegisterAsync_ReturnsOk_WhenRegistrationSucceeds()
    {
        var expectedResponse = new AuthResponse
        {
            AccessToken = "token",
            User = new User { Id = Guid.NewGuid(), Email = "jane@example.com", DisplayName = "Jane Doe" }
        };

        managerMock
            .Setup(m => m.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var controller = CreateController();

        var result = await controller.RegisterAsync(
            new RegisterRequest { Email = "jane@example.com", DisplayName = "Jane Doe", Password = "supersecret123" },
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsBadRequest_WhenManagerThrowsArgumentException()
    {
        managerMock
            .Setup(m => m.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("invalid payload"));

        var controller = CreateController();

        var result = await controller.RegisterAsync(
            new RegisterRequest { Email = "", DisplayName = "", Password = "" },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("invalid payload", badRequest.Value);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsBadRequest_WhenManagerThrowsInvalidOperationException()
    {
        managerMock
            .Setup(m => m.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("email already exists"));

        var controller = CreateController();

        var result = await controller.RegisterAsync(
            new RegisterRequest { Email = "jane@example.com", DisplayName = "Jane Doe", Password = "supersecret123" },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("email already exists", badRequest.Value);
    }

    [Fact]
    public async Task LoginAsync_ReturnsOk_WhenCredentialsAreValid()
    {
        var expectedResponse = new AuthResponse
        {
            AccessToken = "token",
            User = new User { Id = Guid.NewGuid(), Email = "jane@example.com", DisplayName = "Jane Doe" }
        };

        managerMock
            .Setup(m => m.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var controller = CreateController();

        var result = await controller.LoginAsync(
            new LoginRequest { Email = "jane@example.com", Password = "supersecret123" },
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task LoginAsync_ReturnsBadRequest_WhenManagerThrowsInvalidOperationException()
    {
        managerMock
            .Setup(m => m.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid email or password."));

        var controller = CreateController();

        var result = await controller.LoginAsync(
            new LoginRequest { Email = "jane@example.com", Password = "wrong" },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid email or password.", badRequest.Value);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ReturnsOk_WhenUserIsAuthenticated()
    {
        var userId = Guid.NewGuid();
        var expectedUser = new User { Id = userId, Email = "jane@example.com", DisplayName = "Jane Doe" };

        managerMock
            .Setup(m => m.GetCurrentUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        var controller = CreateController(userId);

        var result = await controller.GetCurrentUserAsync(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedUser, okResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ReturnsUnauthorized_WhenNoUserClaimPresent()
    {
        var controller = CreateController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        var result = await controller.GetCurrentUserAsync(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
        managerMock.Verify(
            m => m.GetCurrentUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ReturnsNotFound_WhenManagerThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        managerMock
            .Setup(m => m.GetCurrentUserAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        var controller = CreateController(userId);

        var result = await controller.GetCurrentUserAsync(CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    private AuthController CreateController(Guid? userId = null)
    {
        var controller = new AuthController(managerMock.Object);

        var claims = userId.HasValue
            ? new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) }
            : Array.Empty<Claim>();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, userId.HasValue ? "TestAuth" : null))
            }
        };

        return controller;
    }
}
