using MediatR;

namespace ColeccionaloYa.Application.Payments.GetOrderPayments;

public record GetOrderPaymentsQuery(int OrderId, int RequesterId, bool IsAdmin) : IRequest<List<PaymentDto>>;
