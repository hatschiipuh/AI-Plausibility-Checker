namespace BusinessService.Api.Contracts;

public class ValidationListItem
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}