using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Gallery.Exceptions;

public sealed class GalleryImageNotFoundException : DomainException {
	public GalleryImageNotFoundException(int id)
		: base(HttpStatusCode.NotFound,
			   "GalleryImageNotFound",
			   $"Gallery image #{id} not found.") { }
}
