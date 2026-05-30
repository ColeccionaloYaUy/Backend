using MediatR;

namespace ColeccionaloYa.Application.Payments.GetOrderPaymentStatus;

public record GetOrderPaymentStatusQuery(int OrderId, int RequesterId, bool IsAdmin) : IRequest<PaymentStatusResultDto>;
