using BusinessService.Api.Contracts;
using BusinessService.Api.Security;
using BusinessService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using User = BusinessService.Domain.User;

namespace BusinessService.Api.Managers.Tests;

public sealed class AuthManagerTests
{
    [Fact]
    public async Task RegisterAsync_CreatesUser_WhenRequestIsValid()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        var request = new RegisterRequest
        {
            Email = "Jane.Doe@Example.com",
            DisplayName = "Jane Doe",
            Password = "supersecret123"
        };

        var response = await authManager.RegisterAsync(request, CancellationToken.None);

        Assert.NotNull(response.AccessToken);
        Assert.Equal("jane.doe@example.com", response.User.Email);
        Assert.Equal("Jane Doe", response.User.DisplayName);

        var storedUser = await dbContext.Users.SingleAsync();
        Assert.Equal("jane.doe@example.com", storedUser.Email);
        Assert.NotEqual(request.Password, storedUser.PasswordHash);
    }

    [Theory]
    [InlineData("", "Jane Doe", "supersecret123")]
    [InlineData("jane@example.com", "", "supersecret123")]
    [InlineData("jane@example.com", "Jane Doe", "")]
    public async Task RegisterAsync_ThrowsArgumentException_WhenRequiredFieldsMissing(
        string email, string displayName, string password)
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        var request = new RegisterRequest
        {
            Email = email,
            DisplayName = displayName,
            Password = password
        };

        await Assert.ThrowsAsync<ArgumentException>(() => authManager.RegisterAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_ThrowsArgumentException_WhenPasswordTooShort()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        var request = new RegisterRequest
        {
            Email = "jane@example.com",
            DisplayName = "Jane Doe",
            Password = "short"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => authManager.RegisterAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_ThrowsInvalidOperationException_WhenEmailAlreadyExists()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        var request = new RegisterRequest
        {
            Email = "jane@example.com",
            DisplayName = "Jane Doe",
            Password = "supersecret123"
        };

        await authManager.RegisterAsync(request, CancellationToken.None);

        var duplicateRequest = new RegisterRequest
        {
            Email = "JANE@example.com",
            DisplayName = "Jane Doe Again",
            Password = "anothersecret123"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => authManager.RegisterAsync(duplicateRequest, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_ReturnsAuthResponse_WhenCredentialsAreValid()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        await authManager.RegisterAsync(
            new RegisterRequest { Email = "jane@example.com", DisplayName = "Jane Doe", Password = "supersecret123" },
            CancellationToken.None);

        var response = await authManager.LoginAsync(
            new LoginRequest { Email = "jane@example.com", Password = "supersecret123" },
            CancellationToken.None);

        Assert.NotNull(response.AccessToken);
        Assert.Equal("jane@example.com", response.User.Email);
    }

    [Fact]
    public async Task LoginAsync_ThrowsInvalidOperationException_WhenUserDoesNotExist()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        var request = new LoginRequest { Email = "unknown@example.com", Password = "supersecret123" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => authManager.LoginAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_ThrowsInvalidOperationException_WhenPasswordIsWrong()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        await authManager.RegisterAsync(
            new RegisterRequest { Email = "jane@example.com", DisplayName = "Jane Doe", Password = "supersecret123" },
            CancellationToken.None);

        var request = new LoginRequest { Email = "jane@example.com", Password = "wrongpassword" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => authManager.LoginAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetCurrentUserAsync_ReturnsUser_WhenUserExists()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        var registerResponse = await authManager.RegisterAsync(
            new RegisterRequest { Email = "jane@example.com", DisplayName = "Jane Doe", Password = "supersecret123" },
            CancellationToken.None);

        var user = await authManager.GetCurrentUserAsync(registerResponse.User.Id, CancellationToken.None);

        Assert.Equal(registerResponse.User.Id, user.Id);
        Assert.Equal("jane@example.com", user.Email);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ThrowsKeyNotFoundException_WhenUserDoesNotExist()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => authManager.GetCurrentUserAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetCurrentUserAsync_ThrowsArgumentException_WhenCurrentUserIdIsEmpty()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        await Assert.ThrowsAsync<ArgumentException>(
            () => authManager.GetCurrentUserAsync(Guid.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteCurrentUserAsync_RemovesUser_WhenUserExists()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        var registerResponse = await authManager.RegisterAsync(
            new RegisterRequest { Email = "jane@example.com", DisplayName = "Jane Doe", Password = "supersecret123" },
            CancellationToken.None);

        await authManager.DeleteCurrentUserAsync(registerResponse.User.Id, CancellationToken.None);

        var userExists = await dbContext.Users.AnyAsync(user => user.Id == registerResponse.User.Id);
        Assert.False(userExists);
    }

    [Fact]
    public async Task DeleteCurrentUserAsync_RemovesValidationsAndSteps_WhenUserHasValidations()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        var registerResponse = await authManager.RegisterAsync(
            new RegisterRequest { Email = "jane@example.com", DisplayName = "Jane Doe", Password = "supersecret123" },
            CancellationToken.None);

        var validation = new BusinessService.Domain.Validation
        {
            Id = Guid.NewGuid(),
            UserId = registerResponse.User.Id,
            Question = "Is this true?",
            InitialAnswer = "Yes",
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false
        };
        dbContext.Validations.Add(validation);
        await dbContext.SaveChangesAsync();

        await authManager.DeleteCurrentUserAsync(registerResponse.User.Id, CancellationToken.None);

        var validationExists = await dbContext.Validations.AnyAsync(v => v.Id == validation.Id);
        Assert.False(validationExists);
    }

    [Fact]
    public async Task DeleteCurrentUserAsync_ThrowsKeyNotFoundException_WhenUserDoesNotExist()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => authManager.DeleteCurrentUserAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteCurrentUserAsync_ThrowsArgumentException_WhenCurrentUserIdIsEmpty()
    {
        await using var dbContext = CreateDbContext();
        var authManager = CreateAuthManager(dbContext);

        await Assert.ThrowsAsync<ArgumentException>(
            () => authManager.DeleteCurrentUserAsync(Guid.Empty, CancellationToken.None));
    }

    private static AuthManager CreateAuthManager(BusinessDbContext dbContext)
    {
        var jwtSettings = new JwtSettings
        {
            Issuer = "BusinessService",
            Audience = "ClientApp",
            SigningKey = "unit-test-signing-key-with-enough-length",
            ExpirationMinutes = 120
        };

        return new AuthManager(dbContext, Options.Create(jwtSettings));
    }

    private static BusinessDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BusinessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BusinessDbContext(options);
    }
}
