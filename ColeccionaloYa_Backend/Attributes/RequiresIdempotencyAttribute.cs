namespace ColeccionaloYa.ColeccionaloYa_Backend.Attributes {
    /// <summary>
    /// Marca un endpoint como idempotente.
    /// El <see cref="IdempotencyMiddleware"/> detecta este atributo para exigir
    /// el header "Idempotency-Key" y aplicar la lógica de deduplicación.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    public sealed class RequiresIdempotencyAttribute : Attribute { }
}
