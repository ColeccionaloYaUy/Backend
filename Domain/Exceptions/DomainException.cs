using System.Net;

namespace API.Domain.Exceptions;

public class DomainException : Exception {
	public DomainException(HttpStatusCode statusCode, string errorType, string message) : base(message) {
		ErrorType = errorType;
		StatusCode = statusCode;
	}

	public HttpStatusCode StatusCode { get; }
	public string ErrorType { get; }
}