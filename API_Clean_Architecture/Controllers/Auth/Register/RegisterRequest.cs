using API.API_Clean_Architecture.Attributes;

namespace API.API_Clean_Architecture.Controllers.Auth.Register;

public class RegisterRequest {
	public string Email { get; set; } = string.Empty;

	[IsSensitiveInformation]
	public string Password { get; set; } = string.Empty;

	public string FullName { get; set; } = string.Empty;
}