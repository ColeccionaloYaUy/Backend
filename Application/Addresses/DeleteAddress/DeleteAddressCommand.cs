using MediatR;

namespace ColeccionaloYa.Application.Addresses.DeleteAddress;

public record DeleteAddressCommand(int Id, int RequesterId, bool IsAdmin) : IRequest<Unit>;
