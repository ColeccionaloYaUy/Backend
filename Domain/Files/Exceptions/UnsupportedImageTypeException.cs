using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Files.Exceptions;

public sealed class UnsupportedImageTypeException : DomainException {
	public UnsupportedImageTypeException(string fileName)
		: base(HttpStatusCode.UnsupportedMediaType,
			   "UnsupportedMediaType",
			   $"The file '{fileName}' is not a supported image type.") { }
}
