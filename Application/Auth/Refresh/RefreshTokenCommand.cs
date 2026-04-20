using MediatR;

namespace ColeccionaloYa.Application.Auth.Refresh;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;
