using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Tags.Exceptions;

public sealed class TagInUseException : DomainException {
	public TagInUseException(int id)
		: base(HttpStatusCode.Conflict,
			   "TagInUse",
			   $"Tag #{id} cannot be deleted because it is associated with products or used by menu items.") { }
}
