namespace BusinessService.Api.Contracts;

public class ValidationDetails
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string InitialAnswer { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ValidationStep> Steps { get; set; } = new();
}