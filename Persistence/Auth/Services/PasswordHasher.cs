using ColeccionaloYa.Persistence.Auth.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Auth.Services;

[Injectable(ServiceLifetime.Singleton)]
public class PasswordHasher : IPasswordHasher {
	public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

	public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
