using System.ComponentModel.DataAnnotations;

namespace BusinessService.Api.Contracts;

public class ValidateStepRequest
{
    [Required]
    [StringLength(ValidationInputLimits.MaximumInquiryCharacters)]
    public string Inquiry { get; set; } = string.Empty;

    [Required]
    [StringLength(ValidationInputLimits.MaximumUserAnswerCharacters)]
    public string UserAnswer { get; set; } = string.Empty;
}