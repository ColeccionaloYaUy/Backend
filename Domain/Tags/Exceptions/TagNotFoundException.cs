using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Tags.Exceptions;

public sealed class TagNotFoundException : DomainException {
	public TagNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "TagNotFound",
			   $"Tag #{id} not found.") { }
}
