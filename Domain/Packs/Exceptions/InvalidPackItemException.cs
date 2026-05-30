using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Packs.Exceptions;

public sealed class InvalidPackItemException : DomainException {
	public InvalidPackItemException()
		: base(HttpStatusCode.UnprocessableEntity,
			   "InvalidPackItem",
			   "One or more products do not exist or are packs (packs cannot be nested).") { }
}
