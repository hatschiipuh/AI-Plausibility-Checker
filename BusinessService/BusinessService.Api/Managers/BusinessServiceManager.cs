using BusinessService.Api.Clients;
using BusinessService.Api.Clients.ValidationScoringService;
using BusinessService.Api.Contracts;
using BusinessService.Domain;
using BusinessService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BusinessService.Api.Managers;

public sealed class BusinessServiceManager : IBusinessServiceManager
{
    private readonly BusinessDbContext dbContext;
    private readonly IValidationScoringServiceClient validationScoringServiceClient;

    public BusinessServiceManager(BusinessDbContext dbContext, IValidationScoringServiceClient validationScoringServiceClient)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        this.validationScoringServiceClient = validationScoringServiceClient ?? throw new ArgumentNullException(nameof(validationScoringServiceClient));
    }

    public async Task<UserDto> GetOrCreateUserAsync(GetOrCreateUserRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.DisplayName))
        {
            throw new ArgumentException("Email and display name are required.", nameof(request));
        }

        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                DisplayName = request.DisplayName.Trim()
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return MapUserDto(user);
    }

    public async Task<ValidationDto> CreateValidationAsync(CreateValidationRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Question) || string.IsNullOrWhiteSpace(request.InitialAnswer))
        {
            throw new ArgumentException("UserId, question, and initial answer are required.", nameof(request));
        }

        var userExists = await dbContext.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var validation = new Validation
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Question = request.Question.Trim(),
            InitialAnswer = request.InitialAnswer.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        dbContext.Validations.Add(validation);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapValidationDto(validation);
    }

    public async Task<IReadOnlyList<ValidationListItemDto>> GetValidationsForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("A valid userId is required.", nameof(userId));
        }

        var userExists = await dbContext.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        if (!userExists)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var items = await dbContext.Validations
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new ValidationListItemDto
            {
                Id = v.Id,
                Question = v.Question,
                IsCompleted = v.IsCompleted,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<ValidationDetailsDto> GetValidationDetailsAsync(Guid validationId, CancellationToken cancellationToken)
    {
        if (validationId == Guid.Empty)
        {
            throw new ArgumentException("A valid validationId is required.", nameof(validationId));
        }

        var validation = await dbContext.Validations
            .Include(v => v.Steps)
            .FirstOrDefaultAsync(v => v.Id == validationId, cancellationToken);

        if (validation is null)
        {
            throw new KeyNotFoundException("Validation not found.");
        }

        return MapValidationDetailsDto(validation);
    }

    public async Task<ValidateStepResponse> ValidateStepAsync(Guid validationId, ValidateStepRequest request, CancellationToken cancellationToken)
    {
        if (validationId == Guid.Empty)
        {
            throw new ArgumentException("A valid validationId is required.", nameof(validationId));
        }

        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Inquiry) || string.IsNullOrWhiteSpace(request.UserAnswer))
        {
            throw new ArgumentException("Inquiry and user answer are required.", nameof(request));
        }

        var validation = await dbContext.Validations
            .Include(v => v.Steps)
            .FirstOrDefaultAsync(v => v.Id == validationId, cancellationToken);

        if (validation is null)
        {
            throw new KeyNotFoundException("Validation not found.");
        }

        if (validation.IsCompleted)
        {
            throw new InvalidOperationException("Validation is already completed.");
        }

        var step = new ValidationStep
        {
            Id = Guid.NewGuid(),
            ValidationId = validation.Id,
            CreatedAt = DateTime.UtcNow,
            Inquiry = request.Inquiry.Trim(),
            UserAnswer = request.UserAnswer.Trim(),
            Score = null,
            AiExplanation = null,
            IsFinal = false
        };

        dbContext.ValidationSteps.Add(step);

        var aiRequest = new AiScoreStepRequest
        {
            ValidationId = validation.Id,
            Question = validation.Question,
            InitialAnswer = validation.InitialAnswer,
            Inquiry = step.Inquiry,
            UserAnswer = step.UserAnswer
        };

        var aiResponse = await validationScoringServiceClient.ScoreStepAsync(aiRequest, cancellationToken);

        step.Score = aiResponse.Score;
        step.AiExplanation = aiResponse.AiExplanation;
        step.IsFinal = aiResponse.IsFinal;

        if (aiResponse.IsFinal)
        {
            validation.IsCompleted = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ValidateStepResponse
        {
            ValidationId = validation.Id,
            IsCompleted = validation.IsCompleted,
            Score = aiResponse.Score,
            AiExplanation = aiResponse.AiExplanation,
            NextInquiry = aiResponse.NextInquiry
        };
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static UserDto MapUserDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName
    };

    private static ValidationDto MapValidationDto(Validation validation) => new()
    {
        Id = validation.Id,
        UserId = validation.UserId,
        Question = validation.Question,
        InitialAnswer = validation.InitialAnswer,
        IsCompleted = validation.IsCompleted
    };

    private static ValidationDetailsDto MapValidationDetailsDto(Validation validation)
    {
        return new ValidationDetailsDto
        {
            Id = validation.Id,
            UserId = validation.UserId,
            Question = validation.Question,
            InitialAnswer = validation.InitialAnswer,
            IsCompleted = validation.IsCompleted,
            CreatedAt = validation.CreatedAt,
            Steps = validation.Steps
                .OrderBy(step => step.CreatedAt)
                .Select(step => new ValidationStepDto
                {
                    Id = step.Id,
                    CreatedAt = step.CreatedAt,
                    Inquiry = step.Inquiry,
                    UserAnswer = step.UserAnswer,
                    Score = step.Score,
                    AiExplanation = step.AiExplanation,
                    IsFinal = step.IsFinal
                })
                .ToList()
        };
    }
}
