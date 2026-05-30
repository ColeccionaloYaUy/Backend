using ColeccionaloYa.Domain.Clients.Exceptions;
using ColeccionaloYa.Domain.Coupons.Exceptions;
using ColeccionaloYa.Persistence.Coupons.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Coupons.AssignCouponClients;

public class AssignCouponClientsCommandHandler : IRequestHandler<AssignCouponClientsCommand, CouponClientsDto> {
	private readonly ICouponRepository _Repository;

	public AssignCouponClientsCommandHandler(ICouponRepository repository) {
		_Repository = repository;
	}

	public async Task<CouponClientsDto> Handle(AssignCouponClientsCommand request, CancellationToken cancellationToken) {
		_ = await _Repository.GetByIdAsync(request.CouponId, cancellationToken)
			?? throw new CouponNotFoundException(request.CouponId);

		if (!await _Repository.ClientsExistAsync(request.ClientIds, cancellationToken)) {
			throw new InvalidClientReferenceException();
		}

		if (await _Repository.AnyClientAssignedAsync(request.CouponId, request.ClientIds, cancellationToken)) {
			throw new CouponClientAlreadyAssignedException();
		}

		await _Repository.AssignClientsAsync(request.CouponId, request.ClientIds, cancellationToken);
		var clients = await _Repository.GetAssignedClientsAsync(request.CouponId, cancellationToken);
		return new CouponClientsDto(request.CouponId, clients.Select(CouponClientDto.From).ToList());
	}
}
