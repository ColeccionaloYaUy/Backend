using MediatR;

namespace ColeccionaloYa.Application.Payments.SyncPayment;

public record SyncPaymentCommand(int IdPayment) : IRequest<SyncResultDto>;
