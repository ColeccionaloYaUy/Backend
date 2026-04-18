namespace API.API_Clean_Architecture.Models.Logging;

public class HttpLogEntry {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TraceId { get; set; }
    public string Method { get; set; }
    public string Scheme { get; set; }
    public string Host { get; set; }
    public string Path { get; set; }
    public string Query { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public string Body { get; set; }
    public long? ContentLength { get; set; }
    public string Protocol { get; set; }
    public string ClientIP { get; set; }
    public int RemotePort { get; set; }
    public string User { get; set; }
    public List<KeyValuePair<string, string>> Claims { get; set; }
    public DateTime RequestTimeUtc { get; set; }
    public int StatusCode { get; set; }
    public Dictionary<string, string> ResponseHeaders { get; set; }
    public string ResponseBody { get; set; }
    public long? ResponseContentLength { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public DateTime ResponseTimeUtc { get; set; }
    public string MachineName { get; set; }
    public string EnvironmentName { get; set; }
}