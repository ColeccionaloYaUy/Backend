using MediatR;

namespace ColeccionaloYa.Application.Payments.RetryPayment;

public record RetryPaymentCommand(int IdPayment, int RequesterId, bool IsAdmin) : IRequest<CreatePreferenceResultDto>;
