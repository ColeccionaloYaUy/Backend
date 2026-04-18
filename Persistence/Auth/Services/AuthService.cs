using API.DataAccess.Interfaces;
using API.Domain.Auth;
using API.Persistence.Auth.Interfaces;
using API.Utils.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Persistence.Auth.Services;

[Injectable(ServiceLifetime.Scoped)]
public class AuthService : IAuthService {
	private readonly ICConnection _Connection;
	private readonly IJwtService _JwtService;
	private readonly int _RefreshTokenDays;

	public AuthService(ICConnection connection, IJwtService jwtService, IConfiguration configuration) {
		_Connection = connection;
		_JwtService = jwtService;
		_RefreshTokenDays = int.Parse(configuration["Auth:RefreshTokenExpirationDays"] ?? "30");
	}

	public async Task<AuthData?> LoginAsync(string email, string password) {
		

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT u.id, u.email, u.password_hash, r.name AS role_name
            FROM users u
            INNER JOIN roles r ON r.id = u.role_id
            WHERE u.email = @email AND u.active = TRUE";
		cmd.AddParameter("email", email);

		var user = await cmd.ExecuteSelect<UserRow>((obj, rs) => {
			obj.Id = rs.GetValue<int>("id");
			obj.Email = rs.GetValue<string>("email");
			obj.PasswordHash = rs.GetValue<string>("password_hash");
			obj.RoleName = rs.GetValue<string>("role_name");
		});

		if (user == null) return null;
		if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

		return await CreateSession(user.Id, user.Email, user.RoleName);
	}

	public async Task<AuthData?> RegisterAsync(string email, string password, string name) {
		

		var checkCmd = _Connection.CreateCommand();
		checkCmd.CommandText = "SELECT id FROM users WHERE email = @email";
		checkCmd.AddParameter("email", email);
		if (await checkCmd.ExecuteCommandExists()) return null;

		var roleCmd = _Connection.CreateCommand();
		roleCmd.CommandText = "SELECT id FROM roles WHERE name = 'User' LIMIT 1";
		var roleId = await roleCmd.ExecuteGetValue<int>("id");
		if (roleId == 0) return null;

		var hash = BCrypt.Net.BCrypt.HashPassword(password);
		var insertCmd = _Connection.CreateCommand();
		insertCmd.CommandText = @"
            INSERT INTO users (name, email, password_hash, role_id)
            VALUES (@name, @email, @hash, @roleId)
            RETURNING id";
		insertCmd.AddParameter("name", name);
		insertCmd.AddParameter("email", email);
		insertCmd.AddParameter("hash", hash);
		insertCmd.AddParameter("roleId", roleId);
		var newId = await insertCmd.ExecuteGetValue<int>("id");
		if (newId == 0) return null;

		return await CreateSession(newId, email, "User");
	}

	public async Task<AuthData?> RefreshTokenAsync(string token, string refreshToken) {
		

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT rt.id, u.id AS user_id, u.email, r.name AS role_name
            FROM refresh_tokens rt
            INNER JOIN users u ON u.id = rt.user_id
            INNER JOIN roles r ON r.id = u.role_id
            WHERE rt.token = @token
              AND rt.revoked = FALSE
              AND rt.expires_at > NOW()
              AND u.active = TRUE";
		cmd.AddParameter("token", refreshToken);

		var data = await cmd.ExecuteSelect<RefreshRow>((obj, rs) => {
			obj.RefreshTokenId = rs.GetValue<int>("id");
			obj.UserId = rs.GetValue<int>("user_id");
			obj.Email = rs.GetValue<string>("email");
			obj.RoleName = rs.GetValue<string>("role_name");
		});

		if (data == null) return null;

		var revokeCmd = _Connection.CreateCommand();
		revokeCmd.CommandText = "UPDATE refresh_tokens SET revoked = TRUE WHERE id = @id";
		revokeCmd.AddParameter("id", data.RefreshTokenId);
		await revokeCmd.ExecuteCommandNonQuery();

		return await CreateSession(data.UserId, data.Email, data.RoleName);
	}

	public async Task<bool> LogoutAsync(string token, string refreshToken) {
		

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "UPDATE refresh_tokens SET revoked = TRUE WHERE token = @token";
		cmd.AddParameter("token", refreshToken);
		return await cmd.ExecuteCommandNonQuery();
	}

	private async Task<AuthData> CreateSession(int userId, string email, string roleName) {
		var accessToken = _JwtService.GenerateAccessToken(userId, email, roleName);
		var refreshToken = _JwtService.GenerateRefreshToken();
		const int expiresIn = 3600;

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO refresh_tokens (user_id, token, expires_at)
            VALUES (@userId, @token, @expiresAt)";
		cmd.AddParameter("userId", userId);
		cmd.AddParameter("token", refreshToken);
		cmd.AddParameter("expiresAt", DateTime.UtcNow.AddDays(_RefreshTokenDays));
		await cmd.ExecuteCommandNonQuery();

		return new AuthData {
			Token = accessToken,
			RefreshToken = refreshToken,
			ExpiresIn = expiresIn,
		};
	}

	private class UserRow {
		public int Id { get; set; }
		public string Email { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public string RoleName { get; set; } = string.Empty;
	}

	private class RefreshRow {
		public int RefreshTokenId { get; set; }
		public int UserId { get; set; }
		public string Email { get; set; } = string.Empty;
		public string RoleName { get; set; } = string.Empty;
	}
}
