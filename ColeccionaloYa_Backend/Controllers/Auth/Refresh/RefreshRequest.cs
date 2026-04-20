using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Auth.Refresh;

public class RefreshRequest {
	[Required]
	public string RefreshToken { get; set; } = string.Empty;
}
