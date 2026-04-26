namespace BusinessService.Api.Contracts;

public class ValidateStepResponse
{
    public Guid ValidationId { get; set; }
    public bool IsCompleted { get; set; }
    public double Score { get; set; }
    public string AiExplanation { get; set; } = string.Empty;
    public string? NextInquiry { get; set; }
}