using System.ComponentModel.DataAnnotations;

namespace BusinessService.Api.Contracts;

public class CreateValidationRequest
{
    [Required]
    [StringLength(ValidationInputLimits.MaximumQuestionCharacters)]
    public string Question { get; set; } = string.Empty;

    [Required]
    [StringLength(ValidationInputLimits.MaximumInitialAnswerCharacters)]
    public string InitialAnswer { get; set; } = string.Empty;
}