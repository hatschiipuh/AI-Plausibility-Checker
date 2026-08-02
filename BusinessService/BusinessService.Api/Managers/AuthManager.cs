 using System.IdentityModel.Tokens.Jwt;
 using System.Security.Claims;
 using System.Text;
 using BusinessService.Api.Contracts;
 using BusinessService.Api.Security;
 using BusinessService.Infrastructure;
 using Microsoft.AspNetCore.Identity;
 using Microsoft.EntityFrameworkCore;
 using Microsoft.Extensions.Options;
 using Microsoft.IdentityModel.Tokens;
 using DomainUser = BusinessService.Domain.User;
 
 namespace BusinessService.Api.Managers;
 
 public sealed class AuthManager : IAuthManager
 {
     private readonly BusinessDbContext dbContext;
     private readonly JwtSettings jwtSettings;
     private readonly PasswordHasher<DomainUser> passwordHasher = new();
 
     public AuthManager(BusinessDbContext dbContext, IOptions<JwtSettings> jwtOptions)
     {
         this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
         this.jwtSettings = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
     }
 
     public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
     {
         ArgumentNullException.ThrowIfNull(request);
 
         if (string.IsNullOrWhiteSpace(request.Email) ||
             string.IsNullOrWhiteSpace(request.DisplayName) ||
             string.IsNullOrWhiteSpace(request.Password))
         {
             throw new ArgumentException("Email, display name, and password are required.", nameof(request));
         }
 
         if (request.Password.Trim().Length < 8)
         {
             throw new ArgumentException("Password must be at least 8 characters long.", nameof(request));
         }
 
         var normalizedEmail = NormalizeEmail(request.Email);
         var userExists = await dbContext.Users.AnyAsync(
             user => user.Email == normalizedEmail,
             cancellationToken);
 
         if (userExists)
         {
             throw new InvalidOperationException("A user with this email already exists.");
         }
 
         var user = new DomainUser
         {
             Id = Guid.NewGuid(),
             Email = normalizedEmail,
             DisplayName = request.DisplayName.Trim()
         };
 
         user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
 
         dbContext.Users.Add(user);
         await dbContext.SaveChangesAsync(cancellationToken);
 
         return CreateAuthResponse(user);
     }
 
     public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
     {
         ArgumentNullException.ThrowIfNull(request);
 
         if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
         {
             throw new ArgumentException("Email and password are required.", nameof(request));
         }
 
         var normalizedEmail = NormalizeEmail(request.Email);
         var user = await dbContext.Users.FirstOrDefaultAsync(
             existingUser => existingUser.Email == normalizedEmail,
             cancellationToken);
 
         if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
         {
             throw new InvalidOperationException("Invalid email or password.");
         }
 
         var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
         if (verificationResult == PasswordVerificationResult.Failed)
         {
             throw new InvalidOperationException("Invalid email or password.");
         }
 
         if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
         {
             user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
             await dbContext.SaveChangesAsync(cancellationToken);
         }
 
         return CreateAuthResponse(user);
     }
 
     public async Task<User> GetCurrentUserAsync(Guid currentUserId, CancellationToken cancellationToken)
     {
         if (currentUserId == Guid.Empty)
         {
             throw new ArgumentException("A valid currentUserId is required.", nameof(currentUserId));
         }
 
         var user = await dbContext.Users.FirstOrDefaultAsync(
             existingUser => existingUser.Id == currentUserId,
             cancellationToken);
 
         if (user is null)
         {
             throw new KeyNotFoundException("User not found.");
         }
 
         return MapUser(user);
     }

     public async Task DeleteCurrentUserAsync(Guid currentUserId, CancellationToken cancellationToken)
     {
         if (currentUserId == Guid.Empty)
         {
             throw new ArgumentException("A valid currentUserId is required.", nameof(currentUserId));
         }

         var user = await dbContext.Users.FirstOrDefaultAsync(
             existingUser => existingUser.Id == currentUserId,
             cancellationToken);

         if (user is null)
         {
             throw new KeyNotFoundException("User not found.");
         }

         dbContext.Users.Remove(user);
         await dbContext.SaveChangesAsync(cancellationToken);
     }

     private AuthResponse CreateAuthResponse(DomainUser user) =>
         new()
         {
             AccessToken = CreateAccessToken(user),
             User = MapUser(user)
         };
 
     private string CreateAccessToken(DomainUser user)
     {
         var claims = new List<Claim>
         {
             new(ClaimTypes.NameIdentifier, user.Id.ToString()),
             new(ClaimTypes.Name, user.DisplayName),
             new(ClaimTypes.Email, user.Email),
             new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
             new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
         };
 
         var signingCredentials = new SigningCredentials(
             new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
             SecurityAlgorithms.HmacSha256);
 
         var token = new JwtSecurityToken(
             issuer: jwtSettings.Issuer,
             audience: jwtSettings.Audience,
             claims: claims,
             expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
             signingCredentials: signingCredentials);
 
         return new JwtSecurityTokenHandler().WriteToken(token);
     }
 
     private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
 
     private static User MapUser(DomainUser user) =>
         new()
         {
             Id = user.Id,
             Email = user.Email,
             DisplayName = user.DisplayName
         };
 }