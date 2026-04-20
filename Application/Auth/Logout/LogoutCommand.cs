using MediatR;

namespace ColeccionaloYa.Application.Auth.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Unit>;
