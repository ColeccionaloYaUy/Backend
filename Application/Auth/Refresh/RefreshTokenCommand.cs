using ColeccionaloYa.Domain.Auth;
using MediatR;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Auth.Refresh;

public record RefreshTokenCommand(string Token, string Refresh) : IRequest<AuthResponseDto>;