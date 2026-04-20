using ColeccionaloYa.Domain.Idempotency;

namespace ColeccionaloYa.Persistence.Idempotency.Interfaces {
    public interface IIdempotencyRepository {
        /// <summary>
        /// Intenta insertar una nueva key en estado 'processing'.
        /// Devuelve null si la key no existía (se insertó exitosamente).
        /// Devuelve el registro existente si la key ya existía (válida y no expirada).
        /// </summary>
        Task<IdempotencyKey?> TryInsertAsync(string key, int? clientId, string endpoint, string requestHash, CancellationToken cancellationToken);

        /// <summary>
        /// Marca la key como 'completed' y guarda el resultado.
        /// </summary>
        Task CompleteAsync(string key, int? clientId, int statusCode, string responseBody, CancellationToken cancellationToken);

        /// <summary>
        /// Marca la key como 'failed' (para que el cliente pueda reintentar).
        /// </summary>
        Task FailAsync(string key, int? clientId, CancellationToken cancellationToken);
    }
}
