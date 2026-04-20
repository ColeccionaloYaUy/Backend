using ColeccionaloYa.Persistence.Auth.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit> {
	private readonly IAuthService _AuthService;

	public LogoutCommandHandler(IAuthService authService) {
		_AuthService = authService;
	}

	public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken) {
		await _AuthService.LogoutAsync(request.RefreshToken, cancellationToken);
		return Unit.Value;
	}
}
