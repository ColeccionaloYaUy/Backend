using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Genres.Exceptions;

public sealed class DuplicateGenreNameException : DomainException {
	public DuplicateGenreNameException(string name)
		: base(HttpStatusCode.Conflict,
			   "DuplicateGenreName",
			   $"A genre with the name '{name}' already exists.") { }
}
