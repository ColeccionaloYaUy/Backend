using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Genres.Exceptions;

public sealed class GenreNotFoundException : DomainException {
	public GenreNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "GenreNotFound",
			   $"Genre #{id} not found.") { }
}
