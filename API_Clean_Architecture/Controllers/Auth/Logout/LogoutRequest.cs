using System.ComponentModel.DataAnnotations;

namespace API.API_Clean_Architecture.Controllers.Auth.Logout;

public class LogoutRequest {
	[Required]
	public string RefreshToken { get; set; } = string.Empty;
}
