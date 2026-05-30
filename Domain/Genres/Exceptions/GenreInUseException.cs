using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Genres.Exceptions;

public sealed class GenreInUseException : DomainException {
	public GenreInUseException(int id)
		: base(HttpStatusCode.Conflict,
			   "GenreInUse",
			   $"Genre #{id} cannot be deleted because it is linked to one or more books.") { }
}
