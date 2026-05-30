using ColeccionaloYa.Application.Shared;
using ColeccionaloYa.Domain.Payments;
using ColeccionaloYa.Persistence.Payments.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Payments.GetPayments;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, PagedResult<PaymentListItemDto>> {
	private readonly IPaymentRepository _Repository;

	public GetPaymentsQueryHandler(IPaymentRepository repository) {
		_Repository = repository;
	}

	public async Task<PagedResult<PaymentListItemDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken) {
		var status = request.Status is null ? null : PaymentStatusDb.ToDb(PaymentStatusDb.FromDb(request.Status));
		var data = await _Repository.GetPagedAsync(request.Page, request.PageSize, status, request.OrderId, request.ClientId, request.Provider, request.DateFrom, request.DateTo, cancellationToken);
		return data.ToPagedResult(request.PageSize, PaymentListItemDto.From);
	}
}
