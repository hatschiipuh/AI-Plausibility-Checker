using System;

namespace ValidationScoringService.Api.Contracts;

public class AiScoreStepRequest
{
    public Guid ValidationId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string InitialAnswer { get; set; } = string.Empty;
    public string Inquiry { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
}

public class AiScoreStepResponse
{
    public double Score { get; set; }
    public string AiExplanation { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public string? NextInquiry { get; set; }
}
