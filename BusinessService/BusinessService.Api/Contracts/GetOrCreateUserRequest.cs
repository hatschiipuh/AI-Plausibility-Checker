namespace BusinessService.Api.Contracts;

public class GetOrCreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}