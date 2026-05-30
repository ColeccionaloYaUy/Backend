using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Authors.Exceptions;

public sealed class DuplicateAuthorNameException : DomainException {
	public DuplicateAuthorNameException(string name)
		: base(HttpStatusCode.Conflict,
			   "DuplicateAuthorName",
			   $"An author with the name '{name}' already exists.") { }
}
