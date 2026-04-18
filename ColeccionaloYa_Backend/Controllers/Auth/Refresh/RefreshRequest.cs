using System.ComponentModel.DataAnnotations;

namespace API.API_Clean_Architecture.Controllers.Auth.Refresh;

public class RefreshRequest {
	[Required]
	public string Token { get; set; } = string.Empty;

	[Required]
	public string RefreshToken { get; set; } = string.Empty;
}
