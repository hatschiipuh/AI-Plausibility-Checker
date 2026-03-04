using BusinessService.Api.Clients;
using BusinessService.Api.Contracts;
using BusinessService.Api.Managers;
using BusinessService.Domain;
using BusinessService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BusinessService.Api.Managers.Tests;

public sealed class BusinessServiceManagerTests
{
    [Fact]
    public async Task GetOrCreateUserAsync_CreatesUser_WhenEmailIsNew()
    {
        await using var dbContext = CreateDbContext();
        var manager = new BusinessServiceManager(dbContext, Mock.Of<IAiServiceClient>());
        var request = new GetOrCreateUserRequest
        {
            Email = " TestUser@Example.com ",
            DisplayName = "  Jane Doe  "
        };

        var result = await manager.GetOrCreateUserAsync(request, CancellationToken.None);

        Assert.Equal("testuser@example.com", result.Email);
        Assert.Equal("Jane Doe", result.DisplayName);
        Assert.Single(dbContext.Users);
    }

    [Fact]
    public async Task CreateValidationAsync_ThrowsKeyNotFound_WhenUserDoesNotExist()
    {
        await using var dbContext = CreateDbContext();
        var manager = new BusinessServiceManager(dbContext, Mock.Of<IAiServiceClient>());
        var request = new CreateValidationRequest
        {
            UserId = Guid.NewGuid(),
            Question = "Is the answer true?",
            InitialAnswer = "Yes"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => manager.CreateValidationAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task ValidateStepAsync_CompletesValidation_WhenAiMarksFinal()
    {
        await using var dbContext = CreateDbContext();
        var validation = await SeedValidationAsync(dbContext);

        var aiResponse = new AiScoreStepResponse
        {
            Score = 0.82,
            AiExplanation = "looks convincing",
            IsFinal = true,
            NextInquiry = null
        };

        var aiClient = new Mock<IAiServiceClient>();
        aiClient
            .Setup(client => client.ScoreStepAsync(It.IsAny<AiScoreStepRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResponse);

        var manager = new BusinessServiceManager(dbContext, aiClient.Object);
        var request = new ValidateStepRequest
        {
            Inquiry = "Provide more context",
            UserAnswer = "Here is the detail"
        };

        var response = await manager.ValidateStepAsync(validation.Id, request, CancellationToken.None);

        Assert.True(response.IsCompleted);
        Assert.Equal(aiResponse.Score, response.Score);
        Assert.Equal(aiResponse.AiExplanation, response.AiExplanation);

        var reloaded = await dbContext.Validations
            .Include(v => v.Steps)
            .FirstAsync(v => v.Id == validation.Id, CancellationToken.None);

        Assert.True(reloaded.IsCompleted);
        var step = Assert.Single(reloaded.Steps);
        Assert.Equal(aiResponse.Score, step.Score);
        Assert.Equal(aiResponse.AiExplanation, step.AiExplanation);
        Assert.Equal(request.Inquiry, step.Inquiry);
        Assert.Equal(request.UserAnswer, step.UserAnswer);
    }

    private static async Task<Validation> SeedValidationAsync(BusinessDbContext dbContext)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            DisplayName = "Existing User"
        };

        var validation = new Validation
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Question = "Initial question?",
            InitialAnswer = "Initial answer",
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        dbContext.Users.Add(user);
        dbContext.Validations.Add(validation);
        await dbContext.SaveChangesAsync();

        return validation;
    }

    private static BusinessDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BusinessDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BusinessDbContext(options);
    }
}
