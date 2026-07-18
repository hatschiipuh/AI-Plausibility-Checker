
using BusinessService.Api.Clients.ValidationScoringService;
using BusinessService.Api.Contracts;
using BusinessService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using User = BusinessService.Domain.User;
using Validation = BusinessService.Domain.Validation;
namespace BusinessService.Api.Managers.Tests;

public sealed class BusinessServiceManagerTests
{
    [Fact]
    public async Task CreateValidationAsync_CreatesValidation_ForCurrentUser()
    {
        await using var dbContext = CreateDbContext();
        var manager = new BusinessServiceManager(dbContext, Mock.Of<IValidationScoringServiceClient>());
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            DisplayName = "Jane Doe"
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var request = new CreateValidationRequest
        {
            Question = "Is the answer true?",
            InitialAnswer = "Yes"
        };

        var result = await manager.CreateValidationAsync(user.Id, request, CancellationToken.None);

        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(request.Question, result.Question);
        Assert.Equal(request.InitialAnswer, result.InitialAnswer);
        Assert.Single(dbContext.Validations);
    }

    [Fact]
    public async Task CreateValidationAsync_ThrowsArgumentException_WhenCurrentUserIdIsEmpty()
    {
        await using var dbContext = CreateDbContext();
        var manager = new BusinessServiceManager(dbContext, Mock.Of<IValidationScoringServiceClient>());
        var request = new CreateValidationRequest
        {
            Question = "Is the answer true?",
            InitialAnswer = "Yes"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => manager.CreateValidationAsync(Guid.Empty, request, CancellationToken.None));
    }

    [Fact]
    public async Task ValidateStepAsync_CompletesValidation_WhenAiMarksFinal()
    {
        await using var dbContext = CreateDbContext();
        var validation = await SeedValidationAsync(dbContext);

        var aiResponse = new VSScoreStepResponse
        {
            Score = 0.82,
            AiExplanation = "looks convincing",
            IsFinal = true,
            NextInquiry = null
        };

        var aiClient = new Mock<IValidationScoringServiceClient>();
        VSScoreStepRequest? capturedRequest = null;
        aiClient
            .Setup(client => client.ScoreStepAsync(It.IsAny<VSScoreStepRequest>(), It.IsAny<CancellationToken>()))
            .Callback<VSScoreStepRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(aiResponse);

        var manager = new BusinessServiceManager(dbContext, aiClient.Object);
        var request = new ValidateStepRequest
        {
            Inquiry = "Provide more context",
            UserAnswer = "Here is the detail"
        };

        var response = await manager.ValidateStepAsync(validation.UserId, validation.Id, request, CancellationToken.None);

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
        Assert.NotNull(capturedRequest);
        var inquiry = Assert.Single(capturedRequest!.Inquiries);
        Assert.Equal(request.Inquiry, inquiry.Inquiry);
        Assert.Equal(request.UserAnswer, inquiry.UserAnswer);
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
