namespace BusinessService.Api.Contracts;

public class ValidationStep
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Inquiry { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
    public double? Score { get; set; }
    public string? AiExplanation { get; set; }
    public bool IsFinal { get; set; }
}