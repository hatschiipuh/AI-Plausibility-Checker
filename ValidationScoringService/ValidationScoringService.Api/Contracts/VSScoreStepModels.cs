using System;
using System.ComponentModel.DataAnnotations;

namespace ValidationScoringService.Api.Contracts;

public class VSScoreStepRequest
{
    public Guid ValidationId { get; set; }

    [Required]
    [StringLength(ValidationInputLimits.MaximumQuestionCharacters)]
    public string Question { get; set; } = string.Empty;

    [Required]
    [StringLength(ValidationInputLimits.MaximumInitialAnswerCharacters)]
    public string InitialAnswer { get; set; } = string.Empty;

    [Required]
    [MaxLength(ValidationInputLimits.MaximumFollowUpQuestions)]
    public List<VSScoreStepRequestInquiry> Inquiries { get; set; } = new List<VSScoreStepRequestInquiry>();
}

public class VSScoreStepRequestInquiry
{
    [Required]
    [StringLength(ValidationInputLimits.MaximumInquiryCharacters)]
    public string Inquiry { get; set; } = string.Empty;

    [Required]
    [StringLength(ValidationInputLimits.MaximumUserAnswerCharacters)]
    public string UserAnswer { get; set; } = string.Empty;
}

public class VSScoreStepResponse
{
    public double Score { get; set; }
    public string AiExplanation { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public string? NextInquiry { get; set; }
}
