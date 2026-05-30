using MediatR;

namespace ColeccionaloYa.Application.Payments.CancelPayment;

public record CancelPaymentCommand(int IdPayment, string? Reason) : IRequest<CancelResultDto>;
