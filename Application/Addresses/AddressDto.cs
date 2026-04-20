using ColeccionaloYa.Domain.Addresses;

namespace ColeccionaloYa.Application.Addresses;

public record AddressDto(
	int Id,
	int ClientId,
	string Street,
	string Number,
	string City,
	string Department,
	string PostalCode,
	string Type
) {
	public static AddressDto From(Address a) =>
		new(a.Id, a.ClientId, a.Street, a.Number, a.City, a.Department, a.PostalCode, a.Type.ToString());
}
