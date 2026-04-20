using System.Security.Claims;
using ColeccionaloYa.API_Clean_Architecture.Controllers.Auth.ChangePassword;
using ColeccionaloYa.API_Clean_Architecture.Controllers.Auth.Login;
using ColeccionaloYa.API_Clean_Architecture.Controllers.Auth.Logout;
using ColeccionaloYa.API_Clean_Architecture.Controllers.Auth.Refresh;
using ColeccionaloYa.API_Clean_Architecture.Controllers.Auth.Register;
using ColeccionaloYa.Application.Auth;
using ColeccionaloYa.Application.Auth.ChangePassword;
using ColeccionaloYa.Application.Auth.Login;
using ColeccionaloYa.Application.Auth.Logout;
using ColeccionaloYa.Application.Auth.Refresh;
using ColeccionaloYa.Application.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Auth;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase {
	private readonly IMediator _Mediator;

	public AuthController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpPost("login")]
	public Task<AuthResponseDto> Login([FromBody] LoginRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken);

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken) {
		await _Mediator.Send(new RegisterCommand(request.Name, request.Lastname, request.Email, request.Password), cancellationToken);
		return NoContent();
	}

	[HttpPost("refresh")]
	public Task<AuthResponseDto> RefreshToken([FromBody] RefreshRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken);

	[HttpPost("logout")]
	[Authorize]
	public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken) {
		await _Mediator.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
		return NoContent();
	}

	[HttpPost("change-password")]
	[Authorize]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken) {
		var clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
		await _Mediator.Send(new ChangePasswordCommand(clientId, request.CurrentPassword, request.NewPassword), cancellationToken);
		return NoContent();
	}
}
