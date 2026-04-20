using ColeccionaloYa.Domain.Roles;

namespace ColeccionaloYa.Application.Roles;

public record RoleDto(int Id, string Name, string? Description) {
	public static RoleDto From(Role role) =>
		new(role.Id, role.Name, role.Description);
}
