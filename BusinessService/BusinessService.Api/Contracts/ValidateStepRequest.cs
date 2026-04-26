namespace BusinessService.Api.Contracts;

public class ValidateStepRequest
{
    public string Inquiry { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
}