using ColeccionaloYa.Persistence.Auth.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Auth.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Unit> {
	private readonly IAuthService _AuthService;

	public RegisterCommandHandler(IAuthService authService) {
		_AuthService = authService;
	}

	public async Task<Unit> Handle(RegisterCommand request, CancellationToken cancellationToken) {
		await _AuthService.RegisterAsync(request.Email, request.Password, request.Name, request.Lastname);
		return Unit.Value;
	}
}
