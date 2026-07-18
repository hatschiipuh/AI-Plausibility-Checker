 using System.ComponentModel.DataAnnotations;
 
 namespace BusinessService.Api.Security;
 
 public sealed class JwtSettings
 {
     [Required]
     public string Issuer { get; init; } = string.Empty;
 
     [Required]
     public string Audience { get; init; } = string.Empty;
 
     [Required]
     [MinLength(32)]
     public string SigningKey { get; init; } = string.Empty;
 
     [Range(1, 1440)]
     public int ExpirationMinutes { get; init; } = 120;
 }