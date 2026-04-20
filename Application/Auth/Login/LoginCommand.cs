using MediatR;

namespace ColeccionaloYa.Application.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
