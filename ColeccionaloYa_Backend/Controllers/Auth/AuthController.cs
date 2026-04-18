using API.API_Clean_Architecture.Controllers.Auth.Login;
using API.API_Clean_Architecture.Controllers.Auth.Logout;
using API.API_Clean_Architecture.Controllers.Auth.Refresh;
using API.API_Clean_Architecture.Controllers.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.API_Clean_Architecture.Controllers.Auth;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase {
	private readonly IMediator _Mediator;

	public AuthController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpPost("login")]
	public async Task<AuthResponseDto> Login([FromBody] LoginRequest request) {
		var result = await _Mediator.Send(new LoginCommand(request.Email, request.Password));
		if (result == null) throw new UnauthorizedAccessException();

		return result;
	}

	[HttpPost("register")]
	public async Task Register([FromBody] RegisterRequest request) {
		var result = await _Mediator.Send(new RegisterCommand(request.Email, request.Password, request.FullName));
		if (result == null) throw new InvalidOperationException("Email already in use.");
	}

	[HttpPost("refresh")]
	public async Task<AuthResponseDto> RefreshToken([FromBody] RefreshRequest request) {
		var result = await _Mediator.Send(new RefreshTokenCommand(request.Token, request.RefreshToken));
		if (result == null) throw new UnauthorizedAccessException();

		return result;
	}

	[HttpPost("logout")]
	[Authorize]
	public async Task Logout([FromBody] LogoutRequest request) {
		var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
		await _Mediator.Send(new LogoutCommand(token, request.RefreshToken));
	}
}
