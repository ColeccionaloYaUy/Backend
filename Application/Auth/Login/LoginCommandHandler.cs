using ColeccionaloYa.Persistence.Auth.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto> {
	private readonly IAuthService _AuthService;

	public LoginCommandHandler(IAuthService authService) {
		_AuthService = authService;
	}

	public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken) {
		var authData = await _AuthService.LoginAsync(request.Email, request.Password, cancellationToken);
		return AuthResponseDto.From(authData);
	}
}
