using ColeccionaloYa.Domain.Auth;

namespace ColeccionaloYa.Persistence.Auth.Interfaces;

public interface IAuthService {
	Task<AuthData> LoginAsync(string email, string password);
	Task<AuthData> RegisterAsync(string email, string password, string name, string lastname);
	Task<AuthData> RefreshTokenAsync(string refreshToken);
	Task LogoutAsync(string refreshToken);
	Task ChangePasswordAsync(int clientId, string currentPassword, string newPassword);
}
