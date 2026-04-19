using MediatR;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Auth.Logout;

public record LogoutCommand(string Token, string Refresh) : IRequest<bool>;