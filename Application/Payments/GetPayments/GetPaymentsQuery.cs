using ColeccionaloYa.Application.Shared;
using MediatR;

namespace ColeccionaloYa.Application.Payments.GetPayments;

public record GetPaymentsQuery(
	int Page,
	int PageSize,
	string? Status,
	int? OrderId,
	int? ClientId,
	string? Provider,
	DateOnly? DateFrom,
	DateOnly? DateTo
) : IRequest<PagedResult<PaymentListItemDto>>, IPagedQuery;
