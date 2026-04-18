using API.Domain.Auth;
using MediatR;

namespace API.API_Clean_Architecture.Controllers.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;