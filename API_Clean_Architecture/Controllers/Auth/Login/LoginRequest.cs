using System.ComponentModel.DataAnnotations;
using API.API_Clean_Architecture.Attributes;

namespace API.API_Clean_Architecture.Controllers.Auth.Login;

public class LoginRequest {
	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	[Required]
	[MinLength(6)]
	[IsSensitiveInformation]
	public string Password { get; set; } = string.Empty;
}