using ColeccionaloYa.Domain.Auth;

namespace ColeccionaloYa.Application.Auth;

public record AuthResponseDto(string Token, string RefreshToken, long ExpiresIn) {
	public static AuthResponseDto From(AuthData data) =>
		new(data.Token, data.RefreshToken, data.ExpiresIn);
}
