using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Gallery.Exceptions;

public sealed class GalleryImageSourceRequiredException : DomainException {
	public GalleryImageSourceRequiredException()
		: base(HttpStatusCode.UnprocessableEntity,
			   "GalleryImageSourceRequired",
			   "Either an image file or an image url must be provided.") { }
}
