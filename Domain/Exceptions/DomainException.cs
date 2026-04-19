using System.Net;

namespace ColeccionaloYa.Domain.Exceptions;

public abstract class DomainException : Exception {
    public HttpStatusCode StatusCode { get; }
    public string ErrorType { get; }

    protected DomainException(HttpStatusCode statusCode, string errorType, string message)
        : base(message) {
        StatusCode = statusCode;
        ErrorType = errorType;
    }
}