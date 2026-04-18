namespace API.API_Clean_Architecture.Controllers.Auth;

public class AuthResponseDto {
	public string Token { get; set; }
	public string RefreshToken { get; set; }
	public long ExpiresIn { get; set; }

	public AuthResponseDto(string token, string refreshToken, long expiresIn) {
		Token = token;
		RefreshToken = refreshToken;
		ExpiresIn = expiresIn;
	}
}