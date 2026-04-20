using System.ComponentModel.DataAnnotations;
using ColeccionaloYa.API_Clean_Architecture.Attributes;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Clients;

public class CreateClientRequest {
	[Required]
	[MaxLength(100)]
	public string Name { get; set; } = string.Empty;

	[Required]
	[MaxLength(100)]
	public string Lastname { get; set; } = string.Empty;

	[Required]
	[EmailAddress]
	[MaxLength(255)]
	public string Email { get; set; } = string.Empty;

	[MaxLength(25)]
	public string? Phone { get; set; }

	[Required]
	[MinLength(8)]
	[IsSensitiveInformation]
	public string Password { get; set; } = string.Empty;

	[Required]
	public int RoleId { get; set; }

	public bool Active { get; set; } = true;
}
