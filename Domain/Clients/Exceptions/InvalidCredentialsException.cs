using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Clients.Exceptions;

public sealed class InvalidCredentialsException : DomainException {
	public InvalidCredentialsException()
		: base(HttpStatusCode.Unauthorized,
			   "InvalidCredentials",
			   "Invalid email or password.") { }
}
