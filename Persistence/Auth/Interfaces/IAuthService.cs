using ColeccionaloYa.Domain.Auth;

namespace ColeccionaloYa.Persistence.Auth.Interfaces;

public interface IAuthService {
	Task<AuthData> LoginAsync(string email, string password, CancellationToken cancellationToken);
	Task<AuthData> RegisterAsync(string email, string password, string name, string lastname, CancellationToken cancellationToken);
	Task<AuthData> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
	Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
	Task ChangePasswordAsync(int clientId, string currentPassword, string newPassword, CancellationToken cancellationToken);
}
