using MediatR;

namespace ColeccionaloYa.Application.Payments.RefundPayment;

public record RefundPaymentCommand(int IdPayment, decimal? Amount, string? Reason) : IRequest<RefundResultDto>;
