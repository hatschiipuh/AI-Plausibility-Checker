 namespace BusinessService.Api.Contracts;
 
 public class AuthResponse
 {
     public string AccessToken { get; set; } = string.Empty;
     public User User { get; set; } = new();
 }