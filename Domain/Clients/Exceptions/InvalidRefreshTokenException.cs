using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Clients.Exceptions;

public sealed class InvalidRefreshTokenException : DomainException {
	public InvalidRefreshTokenException()
		: base(HttpStatusCode.Unauthorized,
			   "InvalidRefreshToken",
			   "The refresh token is invalid, expired, or revoked.") { }
}
