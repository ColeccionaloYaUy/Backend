using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Roles;

public class RoleRequest {
	[Required]
	[MaxLength(50)]
	public string Name { get; set; } = string.Empty;

	[MaxLength(255)]
	public string? Description { get; set; }
}
