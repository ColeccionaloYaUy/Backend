using System.ComponentModel.DataAnnotations;
using ColeccionaloYa.API_Clean_Architecture.Attributes;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Auth.ChangePassword;

public class ChangePasswordRequest {
	[Required]
	[IsSensitiveInformation]
	public string CurrentPassword { get; set; } = string.Empty;

	[Required]
	[MinLength(8)]
	[IsSensitiveInformation]
	public string NewPassword { get; set; } = string.Empty;
}
