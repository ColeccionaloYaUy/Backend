namespace API.Persistence.Auth.Interfaces;

public interface IJwtService {
	string GenerateAccessToken(int userId, string email, string roleName);
	string GenerateRefreshToken();
	int? GetUserIdFromToken(string token);
}
