using System;
using System.Collections.Generic;

namespace BusinessService.Api.Contracts;

public class GetOrCreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class CreateValidationRequest
{
    public Guid UserId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string InitialAnswer { get; set; } = string.Empty;
}

public class ValidationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string InitialAnswer { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class ValidationListItemDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ValidationStepDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Inquiry { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
    public double? Score { get; set; }
    public string? AiExplanation { get; set; }
    public bool IsFinal { get; set; }
}

public class ValidationDetailsDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string InitialAnswer { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ValidationStepDto> Steps { get; set; } = new();
}

public class ValidateStepRequest
{
    public string Inquiry { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
}

public class ValidateStepResponse
{
    public Guid ValidationId { get; set; }
    public bool IsCompleted { get; set; }
    public double Score { get; set; }
    public string AiExplanation { get; set; } = string.Empty;
    public string? NextInquiry { get; set; }
}
