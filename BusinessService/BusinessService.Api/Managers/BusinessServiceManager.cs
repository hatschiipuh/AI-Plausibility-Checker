using BusinessService.Api.Clients;
using BusinessService.Api.Clients.ValidationScoringService;
using BusinessService.Api.Contracts;
using BusinessService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DomainUser = BusinessService.Domain.User;
using DomainValidation = BusinessService.Domain.Validation;
using DomainValidationStep = BusinessService.Domain.ValidationStep;

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

    public async Task<Validation> CreateValidationAsync(Guid currentUserId, CreateValidationRequest request, CancellationToken cancellationToken)
    {     
        ArgumentNullException.ThrowIfNull(request);

        if (currentUserId == Guid.Empty)
        {
            throw new ArgumentException("A valid currentUserId is required.", nameof(currentUserId));
        }

        if (string.IsNullOrWhiteSpace(request.Question) || string.IsNullOrWhiteSpace(request.InitialAnswer))
        {
            throw new ArgumentException("Question and initial answer are required.", nameof(request));
        }

        if (request.Question.Length > ValidationInputLimits.MaximumQuestionCharacters)
        {
            throw new ArgumentException(
                $"Question cannot exceed {ValidationInputLimits.MaximumQuestionCharacters} characters.",
                nameof(request));
        }

        if (request.InitialAnswer.Length > ValidationInputLimits.MaximumInitialAnswerCharacters)
        {
            throw new ArgumentException(
                $"Initial answer cannot exceed {ValidationInputLimits.MaximumInitialAnswerCharacters} characters.",
                nameof(request));
        }

        var validation = new DomainValidation
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Question = request.Question.Trim(),
            InitialAnswer = request.InitialAnswer.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        dbContext.Validations.Add(validation);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapValidation(validation);
    }

    public async Task<IReadOnlyList<ValidationListItem>> GetValidationsForCurrentUserAsync(Guid currentUserId, CancellationToken cancellationToken)
    {
        if (currentUserId == Guid.Empty)
        {
            throw new ArgumentException("A valid currentUserId is required.", nameof(currentUserId));
        }

        var userExists = await dbContext.Users.AnyAsync(u => u.Id == currentUserId, cancellationToken);
        if (!userExists)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var items = await dbContext.Validations
            .Where(v => v.UserId == currentUserId)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new ValidationListItem
            {
                Id = v.Id,
                Question = v.Question,
                IsCompleted = v.IsCompleted,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return items;
    }

   public async Task<ValidationDetails> GetValidationDetailsAsync(Guid currentUserId, Guid validationId, CancellationToken cancellationToken)    
   {
        if (validationId == Guid.Empty)
        {
            throw new ArgumentException("A valid validationId is required.", nameof(validationId));
        }

        var validation = await dbContext.Validations
            .Include(v => v.Steps)
            .FirstOrDefaultAsync(v => v.Id == validationId && v.UserId == currentUserId, cancellationToken);

        if (validation is null)
        {
            throw new KeyNotFoundException("Validation not found.");
        }

        return MapValidationDetails(validation);
    }

    public async Task<ValidateStepResponse> ValidateStepAsync(Guid currentUserId, Guid validationId, ValidateStepRequest request, CancellationToken cancellationToken)
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

        if (request.Inquiry.Length > ValidationInputLimits.MaximumInquiryCharacters)
        {
            throw new ArgumentException(
                $"Inquiry cannot exceed {ValidationInputLimits.MaximumInquiryCharacters} characters.",
                nameof(request));
        }

        if (request.UserAnswer.Length > ValidationInputLimits.MaximumUserAnswerCharacters)
        {
            throw new ArgumentException(
                $"User answer cannot exceed {ValidationInputLimits.MaximumUserAnswerCharacters} characters.",
                nameof(request));
        }

        var validation = await dbContext.Validations
            .Include(v => v.Steps)
            .FirstOrDefaultAsync(v => v.Id == validationId && v.UserId == currentUserId, cancellationToken);

        if (validation is null)
        {
            throw new KeyNotFoundException("Validation not found.");
        }

        if (validation.Question.Length > ValidationInputLimits.MaximumQuestionCharacters
            || validation.InitialAnswer.Length > ValidationInputLimits.MaximumInitialAnswerCharacters)
        {
            throw new ArgumentException("The stored question or initial answer exceeds the allowed length.");
        }

        if (validation.IsCompleted)
        {
            throw new InvalidOperationException("Validation is already completed.");
        }

        var step = new DomainValidationStep
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

        var orderedSteps = validation.Steps
            .Append(step)
            .DistinctBy(existingStep => existingStep.Id)
            .OrderBy(existingStep => existingStep.CreatedAt)
            .ToList();

        var inquiries = orderedSteps
            .Skip(1)
            .Select(existingStep => new VSScoreStepRequestInquiry
            {
                Inquiry = existingStep.Inquiry,
                UserAnswer = existingStep.UserAnswer
            })
            .ToList();

        if (inquiries.Count > ValidationInputLimits.MaximumFollowUpQuestions)
        {
            throw new ArgumentException(
                $"A validation cannot contain more than {ValidationInputLimits.MaximumFollowUpQuestions} follow-up questions.",
                nameof(request));
        }

        var totalPromptInputCharacters =
            (long)validation.Question.Length
            + validation.InitialAnswer.Length
            + inquiries.Sum(inquiry => (long)inquiry.Inquiry.Length + inquiry.UserAnswer.Length);

        if (totalPromptInputCharacters > ValidationInputLimits.MaximumPromptInputCharacters)
        {
            throw new ArgumentException(
                $"The validation prompt cannot exceed {ValidationInputLimits.MaximumPromptInputCharacters} characters.",
                nameof(request));
        }

        var aiRequest = new VSScoreStepRequest
        {
            ValidationId = validation.Id,
            Question = validation.Question,
            InitialAnswer = validation.InitialAnswer,
            Inquiries = inquiries
        };

        var aiResponse = await validationScoringServiceClient.ScoreStepAsync(aiRequest, cancellationToken);

        dbContext.ValidationSteps.Add(step);
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

    private static User MapUser(DomainUser user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName
    };

    private static Validation MapValidation(DomainValidation validation) => new()
    {
        Id = validation.Id,
        UserId = validation.UserId,
        Question = validation.Question,
        InitialAnswer = validation.InitialAnswer,
        IsCompleted = validation.IsCompleted
    };

    private static ValidationDetails MapValidationDetails(DomainValidation validation)
    {
        return new ValidationDetails
        {
            Id = validation.Id,
            UserId = validation.UserId,
            Question = validation.Question,
            InitialAnswer = validation.InitialAnswer,
            IsCompleted = validation.IsCompleted,
            CreatedAt = validation.CreatedAt,
            Steps = validation.Steps
                .OrderBy(step => step.CreatedAt)
                .Select(step => new ValidationStep
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
