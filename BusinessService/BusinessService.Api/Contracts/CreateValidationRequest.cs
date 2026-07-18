namespace BusinessService.Api.Contracts;

public class CreateValidationRequest
{
    public string Question { get; set; } = string.Empty;
    public string InitialAnswer { get; set; } = string.Empty;
}