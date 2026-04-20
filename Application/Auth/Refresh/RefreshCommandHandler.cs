using ColeccionaloYa.Persistence.Auth.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Auth.Refresh;

public class RefreshCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto> {
	private readonly IAuthService _AuthService;

	public RefreshCommandHandler(IAuthService authService) {
		_AuthService = authService;
	}

	public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken) {
		var authData = await _AuthService.RefreshTokenAsync(request.RefreshToken);
		return AuthResponseDto.From(authData);
	}
}
