using MediatR;

namespace ColeccionaloYa.Application.Addresses.GetAddressById;

public record GetAddressByIdQuery(int Id, int RequesterId, bool IsAdmin) : IRequest<AddressDto>;
