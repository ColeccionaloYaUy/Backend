using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Clients;

public class UpdateMyProfileRequest {
	[Required]
	[MaxLength(100)]
	public string Name { get; set; } = string.Empty;

	[Required]
	[MaxLength(100)]
	public string Lastname { get; set; } = string.Empty;

	[MaxLength(25)]
	public string? Phone { get; set; }
}
