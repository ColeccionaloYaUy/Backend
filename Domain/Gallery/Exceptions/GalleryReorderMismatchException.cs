using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Gallery.Exceptions;

public sealed class GalleryReorderMismatchException : DomainException {
	public GalleryReorderMismatchException()
		: base(HttpStatusCode.UnprocessableEntity,
			   "GalleryReorderMismatch",
			   "One or more gallery images do not belong to the product.") { }
}
