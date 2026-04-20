using MediatR;

namespace ColeccionaloYa.Application.Addresses.GetAddressesByClient;

public record GetAddressesByClientQuery(int ClientId, int RequesterId, bool IsAdmin) : IRequest<List<AddressDto>>;
