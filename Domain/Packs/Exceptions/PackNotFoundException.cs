using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Packs.Exceptions;

public sealed class PackNotFoundException : DomainException {
	public PackNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "PackNotFound",
			   $"Pack #{id} not found.") { }
}
