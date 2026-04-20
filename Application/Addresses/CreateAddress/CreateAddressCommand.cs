using ColeccionaloYa.Domain.Addresses;
using MediatR;

namespace ColeccionaloYa.Application.Addresses.CreateAddress;

public record CreateAddressCommand(
	int ClientId,
	string Street,
	string Number,
	string City,
	string Department,
	string PostalCode,
	AddressType Type,
	int RequesterId,
	bool IsAdmin
) : IRequest<AddressDto>;
