namespace BusinessService.Api.Contracts;

public class Validation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string InitialAnswer { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}