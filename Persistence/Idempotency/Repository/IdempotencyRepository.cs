using ColeccionaloYa.DataAccess.Interfaces;
using ColeccionaloYa.Domain.Idempotency;
using ColeccionaloYa.Persistence.Idempotency.Interfaces;
using ColeccionaloYa.Utils.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Persistence.Idempotency.Repository {
    [Injectable(ServiceLifetime.Scoped)]
    public class IdempotencyRepository : IIdempotencyRepository {
        private readonly ICConnection _connection;

        public IdempotencyRepository(ICConnection connection)
            => _connection = connection;

        private static void Map(IdempotencyKey obj, ICDataReader rs) {
            obj.Id = rs.GetValue<int>("id");
            obj.Key = rs.GetValue<string>("idempotency_key");
            obj.ClientId = rs.GetValue<int?>("id_client");
            obj.Endpoint = rs.GetValue<string>("endpoint");
            obj.RequestHash = rs.GetValue<string>("request_hash");
            obj.Status = rs.GetValue<string>("status");
            obj.StatusCode = rs.GetValue<short?>("status_code");
            obj.ResponseBody = rs.GetValue<string?>("response_body");
            obj.CreatedAt = rs.GetValue<DateTime>("created_at");
            obj.ExpiresAt = rs.GetValue<DateTime>("expires_at");
        }

        public async Task<IdempotencyKey?> TryInsertAsync(
            string key, int? clientId, string endpoint, string requestHash) {
            // Primero intentamos leer si ya existe (no expirada)
            var existing = await GetExistingAsync(key, clientId);
            if (existing != null)
                return existing;

            // Si no existe, intentamos insertar en 'processing'.
            // ON CONFLICT DO NOTHING protege la race condition.
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
            INSERT INTO idempotency_keys
                (idempotency_key, id_client, endpoint, request_hash, status)
            VALUES (@key, @clientId, @endpoint, @hash, 'processing')
            ON CONFLICT (idempotency_key, id_client) DO NOTHING
            RETURNING id, idempotency_key, id_client, endpoint, request_hash,
                      status, status_code, response_body, created_at, expires_at";
            cmd.AddParameter("key", key);
            cmd.AddParameter("clientId", (object?)clientId ?? DBNull.Value);
            cmd.AddParameter("endpoint", endpoint);
            cmd.AddParameter("hash", requestHash);

            // Si ON CONFLICT DO NOTHING disparó, RETURNING devuelve 0 filas.
            // Eso significa que entre nuestra lectura y el insert alguien más insertó.
            // Releemos.
            var inserted = await cmd.ExecuteSelect<IdempotencyKey>(Map);
            if (inserted is null) {
                // Race condition: otro hilo insertó primero. Devolvemos el existente.
                return await GetExistingAsync(key, clientId);
            }

            // Insertado exitosamente como 'processing' → devolvemos null para indicar
            // "procede con la operación real".
            return null;
        }

        private async Task<IdempotencyKey?> GetExistingAsync(string key, int? clientId) {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
            SELECT id, idempotency_key, id_client, endpoint, request_hash,
                   status, status_code, response_body, created_at, expires_at
            FROM idempotency_keys
            WHERE idempotency_key = @key
              AND (id_client = @clientId OR (id_client IS NULL AND @clientId IS NULL))
              AND expires_at > NOW()";
            cmd.AddParameter("key", key);
            cmd.AddParameter("clientId", (object?)clientId ?? DBNull.Value);
            return await cmd.ExecuteSelect<IdempotencyKey>(Map);
        }

        public async Task CompleteAsync(string key, int? clientId, int statusCode, string responseBody) {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
            UPDATE idempotency_keys
            SET status = 'completed',
                status_code = @statusCode,
                response_body = @body
            WHERE idempotency_key = @key
              AND (id_client = @clientId OR (id_client IS NULL AND @clientId IS NULL))";
            cmd.AddParameter("key", key);
            cmd.AddParameter("clientId", (object?)clientId ?? DBNull.Value);
            cmd.AddParameter("statusCode", (short)statusCode);
            cmd.AddParameter("body", responseBody);
            await cmd.ExecuteCommandNonQuery();
        }

        public async Task FailAsync(string key, int? clientId) {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
            UPDATE idempotency_keys
            SET status = 'failed'
            WHERE idempotency_key = @key
              AND (id_client = @clientId OR (id_client IS NULL AND @clientId IS NULL))";
            cmd.AddParameter("key", key);
            cmd.AddParameter("clientId", (object?)clientId ?? DBNull.Value);
            await cmd.ExecuteCommandNonQuery();
        }
    }
}
