using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Tags.Exceptions;

public sealed class DuplicateTagNameException : DomainException {
	public DuplicateTagNameException(string name)
		: base(HttpStatusCode.Conflict,
			   "DuplicateTagName",
			   $"A tag with the name '{name}' already exists.") { }
}
