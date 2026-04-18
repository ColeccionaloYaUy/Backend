using System.Net;

namespace API.Domain.Exceptions;

public class ModelValidationException : DomainException {
	public ModelValidationException(IDictionary<string, string[]> errors)
		: base(HttpStatusCode.UnprocessableEntity, "Validation", "One or more validation errors occurred") {
		Errors = errors;
	}

	public IDictionary<string, string[]> Errors { get; }
}