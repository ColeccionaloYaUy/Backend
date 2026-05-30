using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Packs.Exceptions;

public sealed class PackItemAlreadyExistsException : DomainException {
	public PackItemAlreadyExistsException(int packId, int productId)
		: base(HttpStatusCode.Conflict,
			   "PackItemAlreadyExists",
			   $"Product #{productId} is already part of pack #{packId}.") { }
}
