using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Files.Exceptions;

public sealed class PayloadTooLargeException : DomainException {
	public PayloadTooLargeException(long maxBytes)
		: base(HttpStatusCode.RequestEntityTooLarge,
			   "PayloadTooLarge",
			   $"The file exceeds the maximum allowed size of {maxBytes / (1024 * 1024)} MB.") { }
}
