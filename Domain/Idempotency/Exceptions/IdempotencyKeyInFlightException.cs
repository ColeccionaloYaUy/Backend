using ColeccionaloYa.Domain.Exceptions;
using System.Net;

namespace ColeccionaloYa.Domain.Idempotency.Exceptions {
    public sealed class IdempotencyKeyInFlightException : DomainException {
        public IdempotencyKeyInFlightException()
            : base(HttpStatusCode.Conflict,
                   "IdempotencyKeyInFlight",
                   "A request with this Idempotency-Key is already being processed. " +
                   "Retry with the same key after a short delay.") { }
    }
}
