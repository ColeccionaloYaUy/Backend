using ColeccionaloYa.Domain.Addresses;
using MediatR;

namespace ColeccionaloYa.Application.Addresses.UpdateAddress;

public record UpdateAddressCommand(
	int Id,
	string Street,
	string Number,
	string City,
	string Department,
	string PostalCode,
	AddressType Type,
	int RequesterId,
	bool IsAdmin
) : IRequest<AddressDto>;
