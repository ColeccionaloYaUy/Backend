using API.Domain.Auth;
using MediatR;

namespace API.API_Clean_Architecture.Controllers.Auth.Refresh;

public record RefreshTokenCommand(string Token, string Refresh) : IRequest<AuthResponseDto>;