using ColeccionaloYa.Domain.Auth;
using MediatR;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;