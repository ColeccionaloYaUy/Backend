using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Auth;
using ColeccionaloYa.Domain.Clients;
using ColeccionaloYa.Domain.Clients.Exceptions;
using ColeccionaloYa.Persistence.Auth.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Auth.Services;

[Injectable(ServiceLifetime.Scoped)]
public class AuthService : IAuthService {
	private readonly ICConnection _Connection;
	private readonly IJwtService _JwtService;
	private readonly IPasswordHasher _PasswordHasher;
	private readonly int _RefreshTokenDays;

	public AuthService(ICConnection connection, IJwtService jwtService, IPasswordHasher passwordHasher, IConfiguration configuration) {
		_Connection = connection;
		_JwtService = jwtService;
		_PasswordHasher = passwordHasher;
		_RefreshTokenDays = int.Parse(configuration["Auth:RefreshTokenExpirationDays"] ?? "30");
	}

	public async Task<AuthData> LoginAsync(string email, string password, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT c.id_client, c.email, c.password_hash, r.name AS role_name
            FROM client c
            INNER JOIN roles r ON r.id = c.role_id
            WHERE c.email = @email AND c.active = TRUE AND c.logical_delete = FALSE";
		cmd.AddParameter("email", email);

		var user = await cmd.ExecuteSelect<ClientCredentialsRow>(rs => new ClientCredentialsRow {
			Id = rs.GetValue<int>("id_client"),
			Email = rs.GetValue<string>("email"),
			PasswordHash = rs.GetValue<string>("password_hash"),
			RoleName = rs.GetValue<string>("role_name"),
		}, cancellationToken);

		if (user == null) throw new InvalidCredentialsException();
		if (!_PasswordHasher.Verify(password, user.PasswordHash)) throw new InvalidCredentialsException();

		return await CreateSession(user.Id, user.Email, user.RoleName, cancellationToken);
	}

	public async Task<AuthData> RegisterAsync(string email, string password, string name, string lastname, CancellationToken cancellationToken) {
		var checkCmd = _Connection.CreateCommand();
		checkCmd.CommandText = "SELECT id_client FROM client WHERE email = @email";
		checkCmd.AddParameter("email", email);
		if (await checkCmd.ExecuteCommandExists(cancellationToken)) throw new EmailAlreadyRegisteredException();

		var roleCmd = _Connection.CreateCommand();
		roleCmd.CommandText = "SELECT id, name FROM roles WHERE name = 'User' LIMIT 1";
		var role = await roleCmd.ExecuteSelect<RoleRow>(rs => new RoleRow {
			Id = rs.GetValue<int>("id"),
			Name = rs.GetValue<string>("name"),
		}, cancellationToken);
		if (role == null) throw new DefaultUserRoleNotFoundException();

		var hash = _PasswordHasher.Hash(password);
		var client = Client.Register(name, lastname, email, hash, role.Id, role.Name);

		var insertCmd = _Connection.CreateCommand();
		insertCmd.CommandText = @"
            INSERT INTO client (name, lastname, email, phone, password_hash, role_id, active, creation_date, logical_delete)
            VALUES (@name, @lastname, @email, @phone, @hash, @roleId, @active, @creationDate, @logicalDelete)
            RETURNING id_client";
		insertCmd.AddParameter("name", client.Name);
		insertCmd.AddParameter("lastname", client.Lastname);
		insertCmd.AddParameter("email", client.Email);
		insertCmd.AddParameter("phone", client.Phone ?? string.Empty);
		insertCmd.AddParameter("hash", client.PasswordHash);
		insertCmd.AddParameter("roleId", client.RoleId);
		insertCmd.AddParameter("active", client.Active);
		insertCmd.AddParameter("creationDate", client.CreationDate);
		insertCmd.AddParameter("logicalDelete", client.LogicalDelete);
		var newId = await insertCmd.ExecuteGetValue<int>("id_client", cancellationToken);
		client.AssignId(newId);

		return await CreateSession(client.Id, client.Email, client.RoleName, cancellationToken);
	}

	public async Task<AuthData> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT rt.id, c.id_client, c.email, r.name AS role_name
            FROM refresh_tokens rt
            INNER JOIN client c ON c.id_client = rt.id_client
            INNER JOIN roles r ON r.id = c.role_id
            WHERE rt.token = @token
              AND rt.revoked = FALSE
              AND rt.expires_at > NOW()
              AND c.active = TRUE
              AND c.logical_delete = FALSE";
		cmd.AddParameter("token", refreshToken);

		var data = await cmd.ExecuteSelect<RefreshRow>(rs => new RefreshRow {
			RefreshTokenId = rs.GetValue<int>("id"),
			ClientId = rs.GetValue<int>("id_client"),
			Email = rs.GetValue<string>("email"),
			RoleName = rs.GetValue<string>("role_name"),
		}, cancellationToken);

		if (data == null) throw new InvalidRefreshTokenException();

		var revokeCmd = _Connection.CreateCommand();
		revokeCmd.CommandText = "UPDATE refresh_tokens SET revoked = TRUE WHERE id = @id";
		revokeCmd.AddParameter("id", data.RefreshTokenId);
		await revokeCmd.ExecuteCommandNonQuery(cancellationToken);

		return await CreateSession(data.ClientId, data.Email, data.RoleName, cancellationToken);
	}

	public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = "UPDATE refresh_tokens SET revoked = TRUE WHERE token = @token";
		cmd.AddParameter("token", refreshToken);
		await cmd.ExecuteCommandNonQuery(cancellationToken);
	}

	public async Task ChangePasswordAsync(int clientId, string currentPassword, string newPassword, CancellationToken cancellationToken) {
		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            SELECT id_client, password_hash
            FROM client
            WHERE id_client = @id AND active = TRUE AND logical_delete = FALSE";
		cmd.AddParameter("id", clientId);

		var row = await cmd.ExecuteSelect<ClientCredentialsRow>(rs => new ClientCredentialsRow {
			Id = rs.GetValue<int>("id_client"),
			PasswordHash = rs.GetValue<string>("password_hash"),
		}, cancellationToken);

		if (row == null) throw new ClientNotFoundException();
		if (!_PasswordHasher.Verify(currentPassword, row.PasswordHash)) throw new InvalidCredentialsException();
		if (_PasswordHasher.Verify(newPassword, row.PasswordHash)) throw new SamePasswordException();

		var newHash = _PasswordHasher.Hash(newPassword);
		var updateCmd = _Connection.CreateCommand();
		updateCmd.CommandText = "UPDATE client SET password_hash = @hash WHERE id_client = @id";
		updateCmd.AddParameter("hash", newHash);
		updateCmd.AddParameter("id", clientId);
		await updateCmd.ExecuteCommandNonQuery(cancellationToken);

		var revokeCmd = _Connection.CreateCommand();
		revokeCmd.CommandText = "UPDATE refresh_tokens SET revoked = TRUE WHERE id_client = @id AND revoked = FALSE";
		revokeCmd.AddParameter("id", clientId);
		await revokeCmd.ExecuteCommandNonQuery(cancellationToken);
	}

	private async Task<AuthData> CreateSession(int clientId, string email, string roleName, CancellationToken cancellationToken) {
		var accessToken = _JwtService.GenerateAccessToken(clientId, email, roleName);
		var refreshToken = _JwtService.GenerateRefreshToken();
		const int expiresIn = 3600;

		var cmd = _Connection.CreateCommand();
		cmd.CommandText = @"
            INSERT INTO refresh_tokens (id_client, token, expires_at)
            VALUES (@clientId, @token, @expiresAt)";
		cmd.AddParameter("clientId", clientId);
		cmd.AddParameter("token", refreshToken);
		cmd.AddParameter("expiresAt", DateTime.UtcNow.AddDays(_RefreshTokenDays));
		await cmd.ExecuteCommandNonQuery(cancellationToken);

		return new AuthData {
			Token = accessToken,
			RefreshToken = refreshToken,
			ExpiresIn = expiresIn,
		};
	}

	private class ClientCredentialsRow {
		public int Id { get; set; }
		public string Email { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public string RoleName { get; set; } = string.Empty;
	}

	private class RoleRow {
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	private class RefreshRow {
		public int RefreshTokenId { get; set; }
		public int ClientId { get; set; }
		public string Email { get; set; } = string.Empty;
		public string RoleName { get; set; } = string.Empty;
	}
}
