using API.Persistence.Auth.Interfaces;
using MediatR;

namespace API.API_Clean_Architecture.Controllers.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto?> {
	private readonly IAuthService _AuthService;

	public LoginCommandHandler(IAuthService authService) {
		_AuthService = authService;
	}

	public async Task<AuthResponseDto?> Handle(LoginCommand request, CancellationToken cancellationToken) {
		var authData = await _AuthService.LoginAsync(request.Email, request.Password);
		if (authData == null) return null;

		return new AuthResponseDto(authData.Token, authData.RefreshToken, authData.ExpiresIn);
	}
}