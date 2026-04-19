using ColeccionaloYa.Persistence.Auth.Interfaces;
using MediatR;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool> {
	private readonly IAuthService _AuthService;

	public LogoutCommandHandler(IAuthService authService) {
		_AuthService = authService;
	}

	public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken) {
		return await _AuthService.LogoutAsync(request.Token, request.Refresh);
	}
}