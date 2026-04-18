using MediatR;

namespace API.API_Clean_Architecture.Controllers.Auth.Register;

public record RegisterCommand(string Email, string Password, string Name) : IRequest<AuthResponseDto>;