using API.Persistence.Auth.Interfaces;
using API.Utils.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Persistence.Auth.Services;

[Injectable(ServiceLifetime.Singleton)]
public class JwtService : IJwtService {
	private readonly SymmetricSecurityKey _signingKey;
	private readonly string _issuer;
	private readonly string _audience;
	private readonly int _expirationMinutes;

	public JwtService(IConfiguration configuration) {
		var secret = configuration["Auth:Secret"]!;
		_signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
		_issuer = configuration["Auth:ValidIssuer"]!;
		_audience = configuration["Auth:ValidAudience"]!;
		_expirationMinutes = int.Parse(configuration["Auth:AccessTokenExpirationMinutes"] ?? "60");
	}

	public string GenerateAccessToken(int userId, string email, string roleName) {
		var claims = new[] {
			new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
			new Claim(ClaimTypes.Email, email),
			new Claim(ClaimTypes.Role, roleName),
		};

		var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
		var token = new JwtSecurityToken(
			issuer: _issuer,
			audience: _audience,
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
			signingCredentials: credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public string GenerateRefreshToken() => Guid.NewGuid().ToString();

	public int? GetUserIdFromToken(string token) {
		try {
			var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
			var sub = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier
													 || c.Type == "nameid")?.Value;
			return int.TryParse(sub, out var id) ? id : null;
		} catch {
			return null;
		}
	}
}
