using System;
using System.Collections.Generic;

namespace BusinessService.Domain;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public ICollection<Validation> Validations { get; set; } = new List<Validation>();
}

public class Validation
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Question { get; set; } = string.Empty;
    public string InitialAnswer { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsCompleted { get; set; }

    public ICollection<ValidationStep> Steps { get; set; } = new List<ValidationStep>();
}

public class ValidationStep
{
    public Guid Id { get; set; }

    public Guid ValidationId { get; set; }
    public Validation Validation { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public string Inquiry { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
    public double? Score { get; set; }
    public string? AiExplanation { get; set; }
    public bool IsFinal { get; set; }
}
