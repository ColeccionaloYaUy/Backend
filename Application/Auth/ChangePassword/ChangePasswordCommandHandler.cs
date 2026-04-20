using ColeccionaloYa.Persistence.Auth.Interfaces;
using MediatR;

namespace ColeccionaloYa.Application.Auth.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit> {
	private readonly IAuthService _AuthService;

	public ChangePasswordCommandHandler(IAuthService authService) {
		_AuthService = authService;
	}

	public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken) {
		await _AuthService.ChangePasswordAsync(request.ClientId, request.CurrentPassword, request.NewPassword);
		return Unit.Value;
	}
}
