using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Idempotency.Exceptions {
    public sealed class IdempotencyKeyMismatchException : DomainException {
        public IdempotencyKeyMismatchException()
            : base(HttpStatusCode.UnprocessableEntity,
                   "IdempotencyKeyMismatch",
                   "This Idempotency-Key was already used with a different request body. " +
                   "Use a new key for a different request.") { }
    }
}
