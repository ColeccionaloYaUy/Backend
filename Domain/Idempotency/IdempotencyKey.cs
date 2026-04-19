namespace ColeccionaloYa.Domain.Idempotency {
    public class IdempotencyKey {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public int? ClientId { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string RequestHash { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;  // "processing" | "completed" | "failed"
        public short? StatusCode { get; set; }
        public string? ResponseBody { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
