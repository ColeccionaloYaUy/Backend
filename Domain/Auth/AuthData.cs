namespace API.Domain.Auth;

public class AuthData {
	public string Token { get; set; } = string.Empty;
	public string RefreshToken { get; set; } = string.Empty;
	public long ExpiresIn { get; set; }
}