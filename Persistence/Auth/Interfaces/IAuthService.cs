using API.Domain.Auth;

namespace API.Persistence.Auth.Interfaces;

public interface IAuthService {
	Task<AuthData?> LoginAsync(string email, string password);
	Task<AuthData?> RegisterAsync(string email, string password, string name);
	Task<AuthData?> RefreshTokenAsync(string token, string refreshToken);
	Task<bool> LogoutAsync(string token, string refreshToken);
}