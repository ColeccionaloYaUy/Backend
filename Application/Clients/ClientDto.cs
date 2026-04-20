using ColeccionaloYa.Domain.Clients;

namespace ColeccionaloYa.Application.Clients;

public record ClientDto(
	int Id,
	string Name,
	string Lastname,
	string Email,
	string? Phone,
	int? AddressDeliveryId,
	int? AddressOrderId,
	int RoleId,
	string RoleName,
	bool Active,
	DateTime CreationDate
) {
	public static ClientDto From(Client c) =>
		new(c.Id, c.Name, c.Lastname, c.Email, c.Phone, c.AddressDeliveryId, c.AddressOrderId, c.RoleId, c.RoleName, c.Active, c.CreationDate);
}
