using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Files.Exceptions;

public sealed class UploadedFileNotFoundException : DomainException {
	public UploadedFileNotFoundException(string fileName)
		: base(HttpStatusCode.NotFound,
			   "FileNotFound",
			   $"The file '{fileName}' does not exist in storage.") { }
}
