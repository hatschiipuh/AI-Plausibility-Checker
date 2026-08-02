 using BusinessService.Api.Contracts;
 
 namespace BusinessService.Api.Managers;
 
 public interface IAuthManager
 {
     Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
     Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
     Task<User> GetCurrentUserAsync(Guid currentUserId, CancellationToken cancellationToken);
     Task DeleteCurrentUserAsync(Guid currentUserId, CancellationToken cancellationToken);
 }