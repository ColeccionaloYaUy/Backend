using MediatR;

namespace ColeccionaloYa.Application.Payments.GetPaymentById;

public record GetPaymentByIdQuery(int IdPayment) : IRequest<PaymentDetailDto>;
