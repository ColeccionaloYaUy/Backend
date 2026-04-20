using MediatR;

namespace ColeccionaloYa.Application.Auth.ChangePassword;

public record ChangePasswordCommand(int ClientId, string CurrentPassword, string NewPassword) : IRequest<Unit>;
