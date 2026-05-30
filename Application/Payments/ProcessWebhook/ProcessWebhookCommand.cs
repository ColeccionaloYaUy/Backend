using MediatR;

namespace ColeccionaloYa.Application.Payments.ProcessWebhook;

public record ProcessWebhookCommand(string? ExternalId) : IRequest<Unit>;
