using MediatR;

namespace ColeccionaloYa.Application.Payments.CreatePreference;

public record CreatePreferenceCommand(int OrderId, int RequesterId, bool IsAdmin) : IRequest<CreatePreferenceResultDto>;
