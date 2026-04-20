using ColeccionaloYa.ColeccionaloYa_Backend.Attributes;
using ColeccionaloYa.Domain.Idempotency;
using ColeccionaloYa.Domain.Idempotency.Exceptions;
using ColeccionaloYa.Persistence.Idempotency.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ColeccionaloYa.ColeccionaloYa_Backend.Middlewares {
    /// <summary>
    /// Middleware de idempotencia basado en el header "Idempotency-Key".
    ///
    /// Flujo:
    /// 1. Si el endpoint no está marcado con [RequiresIdempotency], pasa de largo.
    /// 2. Si no hay header Idempotency-Key en un endpoint que lo requiere, devuelve 400.
    /// 3. Busca la key en BD:
    ///    - No existe → inserta en 'processing', ejecuta la pipeline real, guarda resultado.
    ///    - Existe + 'completed' → devuelve resultado cacheado sin ejecutar el handler.
    ///    - Existe + 'processing' → 409 (en vuelo).
    ///    - Existe + 'failed' → trata como nueva (permite reintentar).
    ///    - Body distinto al de la primera request → 422 Mismatch.
    /// </summary>
    public class IdempotencyMiddleware {
        private readonly RequestDelegate _next;

        public IdempotencyMiddleware(RequestDelegate next)
            => _next = next;

        public async Task InvokeAsync(HttpContext context, IIdempotencyRepository repo) {
            // Solo aplica a métodos que mutan estado
            if (!IsMutableMethod(context.Request.Method)) {
                await _next(context);
                return;
            }

            // Verificar si el endpoint está marcado como [RequiresIdempotency]
            var endpoint = context.GetEndpoint();
            var requiresIdempotency = endpoint?
                .Metadata
                .GetMetadata<RequiresIdempotencyAttribute>() is not null;

            if (!requiresIdempotency) {
                await _next(context);
                return;
            }

            // Leer el header
            if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var keyValues)
                || string.IsNullOrWhiteSpace(keyValues)) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new {
                    error = new {
                        code = "MissingIdempotencyKey",
                        message = "This endpoint requires the 'Idempotency-Key' header.",
                        timestamp = DateTime.UtcNow,
                        path = context.Request.Path.Value
                    }
                });
                return;
            }

            var idempotencyKey = keyValues.ToString().Trim();
            if (idempotencyKey.Length > 128) {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new {
                    error = new {
                        code = "InvalidIdempotencyKey",
                        message = "Idempotency-Key must be 128 characters or fewer.",
                        timestamp = DateTime.UtcNow,
                        path = context.Request.Path.Value
                    }
                });
                return;
            }

            // Obtener id del cliente autenticado (null si anónimo)
            int? clientId = null;
            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var parsedId))
                clientId = parsedId;

            // Leer el body para hashear (necesitamos permitir relectura)
            context.Request.EnableBuffering();
            var bodyBytes = await ReadBodyAsync(context.Request);
            var requestHash = ComputeSha256(bodyBytes);
            var endpoint2 = $"{context.Request.Method} {context.Request.Path}";
            var cancellationToken = context.RequestAborted;

            // Intentar insertar/consultar la key
            IdempotencyKey? existing;
            try {
                existing = await repo.TryInsertAsync(idempotencyKey, clientId, endpoint2, requestHash, cancellationToken);
            } catch {
                // Si falla la BD en este punto, dejamos pasar sin idempotencia
                // (degradación controlada: mejor ejecutar que bloquear todo)
                await _next(context);
                return;
            }

            if (existing is null) {
                // Key nueva, procesamos normalmente
                await ProcessAndSaveAsync(context, repo, idempotencyKey, clientId, bodyBytes);
                return;
            }

            // Key existente
            switch (existing.Status) {
                case "processing":
                    throw new IdempotencyKeyInFlightException();

                case "failed":
                    // Permitir reintento: borrar la vieja y procesar de nuevo
                    await repo.FailAsync(idempotencyKey, clientId, cancellationToken); // ya está en failed, re-insert en siguiente request
                                                                    // En este caso simplemente procesamos (la key está en 'failed', no bloquea)
                    await ProcessAndSaveAsync(context, repo, idempotencyKey, clientId, bodyBytes);
                    return;

                case "completed":
                    // Verificar que el body sea el mismo
                    if (existing.RequestHash != requestHash)
                        throw new IdempotencyKeyMismatchException();

                    // Devolver la respuesta cacheada
                    context.Response.StatusCode = existing.StatusCode ?? 200;
                    context.Response.Headers["X-Idempotency-Replayed"] = "true";
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(existing.ResponseBody ?? "{}");
                    return;
            }

            // Fallback (no debería llegar aquí)
            await _next(context);
        }

        private async Task ProcessAndSaveAsync(
            HttpContext context,
            IIdempotencyRepository repo,
            string key,
            int? clientId,
            byte[] bodyBytes) {
            var cancellationToken = context.RequestAborted;

            // Resetear el body para que el controller pueda leerlo
            context.Request.Body.Position = 0;

            // Capturar la respuesta
            var originalBody = context.Response.Body;
            using var captureStream = new MemoryStream();
            context.Response.Body = captureStream;

            bool succeeded = false;
            try {
                await _next(context);
                succeeded = true;
            } finally {
                // Copiar al stream original
                captureStream.Seek(0, SeekOrigin.Begin);
                await captureStream.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
            }

            // Guardar el resultado
            captureStream.Seek(0, SeekOrigin.Begin);
            var responseBody = Encoding.UTF8.GetString(captureStream.ToArray());

            // Tras finalizar la pipeline del request, el CancellationToken del
            // request puede estar ya cancelado (especialmente en 5xx). Persistir
            // el resultado de idempotencia es crítico para no bloquear retries:
            // usamos CancellationToken.None para asegurar el write.
            if (succeeded && context.Response.StatusCode < 500) {
                await repo.CompleteAsync(key, clientId, context.Response.StatusCode, responseBody, CancellationToken.None);
            } else {
                await repo.FailAsync(key, clientId, CancellationToken.None);
            }
        }

        private static async Task<byte[]> ReadBodyAsync(HttpRequest request) {
            request.Body.Position = 0;
            using var ms = new MemoryStream();
            await request.Body.CopyToAsync(ms);
            request.Body.Position = 0;
            return ms.ToArray();
        }

        private static string ComputeSha256(byte[] data) {
            var hash = SHA256.HashData(data);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static bool IsMutableMethod(string method)
            => method is "POST" or "PUT" or "PATCH";
    }
}
