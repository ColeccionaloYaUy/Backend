using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Packs.Exceptions;

public sealed class PackItemNotFoundException : DomainException {
	public PackItemNotFoundException(int packId, int productId)
		: base(HttpStatusCode.NotFound,
			   "PackItemNotFound",
			   $"Product #{productId} is not part of pack #{packId}.") { }
}
