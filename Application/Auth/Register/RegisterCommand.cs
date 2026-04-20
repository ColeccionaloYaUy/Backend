using MediatR;

namespace ColeccionaloYa.Application.Auth.Register;

public record RegisterCommand(string Name, string Lastname, string Email, string Password) : IRequest<Unit>;
