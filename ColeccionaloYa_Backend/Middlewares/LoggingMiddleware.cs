using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;
using API.API_Clean_Architecture.Attributes;
using API.API_Clean_Architecture.Models.Logging;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Serilog;

namespace API.API_Clean_Architecture.Middlewares;

public class LoggingMiddleware {
	private const string SENSITIVE_INFORMATION = "[SENSITIVE_INFORMATION]";
	private readonly JsonSerializerOptions _JsonOptions;
	private readonly RequestDelegate _Next;
	private readonly string[] _SensitiveHeaders = ["Authorization", "Cookie", "Set-Cookie"];

	public LoggingMiddleware(RequestDelegate next) {
		_Next = next;
		_JsonOptions = new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false,
		};
	}

	public async Task InvokeAsync(HttpContext context) {
		var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

		if (path.StartsWith("/swagger") || path.StartsWith("/api/docs")) {
			await _Next(context);
			return;
		}

		var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
		var requestTimeUtc = DateTime.UtcNow;

		context.Request.EnableBuffering();
		var requestBody = await ReadStreamAsync(context.Request.Body);
		context.Request.Body.Position = 0;

		var requestHeaders = context.Request.Headers
			.Where(h => !_SensitiveHeaders.Contains(h.Key))
			.ToDictionary(h => h.Key, h => h.Value.ToString());

		var userClaims = context.User.Claims.Select(c => new KeyValuePair<string, string>(c.Type, c.Value)).ToList();

		var originalResponseBodyStream = context.Response.Body;
		using var responseBodyMs = new MemoryStream();
		context.Response.Body = responseBodyMs;

		var stopwatch = Stopwatch.StartNew();
		try {
			await _Next(context);
		} catch {
			// ignored
		}

		stopwatch.Stop();

		StreamReader? reader;
		GZipStream? decompressedStream = null;

		responseBodyMs.Seek(0, SeekOrigin.Begin);
		if (context.Response.Headers.TryGetValue("Content-Encoding", out var encoding) &&
			encoding.ToString().Equals("gzip", StringComparison.OrdinalIgnoreCase)) {
			decompressedStream = new GZipStream(responseBodyMs, CompressionMode.Decompress, true);
			reader = new StreamReader(decompressedStream);
		} else {
			reader = new StreamReader(responseBodyMs);
		}

		var responseBody = await reader.ReadToEndAsync();
		responseBodyMs.Seek(0, SeekOrigin.Begin);
		await responseBodyMs.CopyToAsync(originalResponseBodyStream);

		var responseHeaders = context.Response.Headers.Where(h => !_SensitiveHeaders.Contains(h.Key))
			.ToDictionary(h => h.Key, h => h.Value.ToString());

		var requestModelType = GetRequestModelType(context);

		var logEntry = new HttpLogEntry {
			TraceId = traceId,
			Method = context.Request.Method,
			Scheme = context.Request.Scheme,
			Host = context.Request.Host.Value,
			Path = context.Request.Path,
			Query = context.Request.QueryString.ToString(),
			Headers = requestHeaders,
			Body = SanitizeJsonString(requestBody, requestModelType),
			ContentLength = context.Request.ContentLength,
			Protocol = context.Request.Protocol,
			ClientIP = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
			RemotePort = context.Connection.RemotePort,
			User = context.User.Identity?.Name ?? string.Empty,
			Claims = userClaims,
			RequestTimeUtc = requestTimeUtc,
			StatusCode = context.Response.StatusCode,
			ResponseHeaders = responseHeaders,
			ResponseBody = SanitizeJsonString(responseBody),
			ResponseContentLength = context.Response.ContentLength,
			ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
			ResponseTimeUtc = DateTime.UtcNow,
			MachineName = Environment.MachineName,
			EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty,
		};

		//TODO: Save in BD

		Log.Information("{@LogEntry}", logEntry);

		reader.Close();
		reader.Dispose();

		if (decompressedStream != null) {
			decompressedStream.Close();
			await decompressedStream.DisposeAsync();
		}
	}

	private static async Task<string> ReadStreamAsync(Stream stream) {
		stream.Seek(0, SeekOrigin.Begin);
		using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
		var body = await reader.ReadToEndAsync();
		stream.Seek(0, SeekOrigin.Begin);
		return body;
	}

	private static Type? GetRequestModelType(HttpContext context) {
		try {
			var endpoint = context.GetEndpoint();
			if (endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>() is ControllerActionDescriptor
				actionDescriptor) {
				var bodyParameter = actionDescriptor.Parameters
					.FirstOrDefault(p => p.BindingInfo?.BindingSource == BindingSource.Body
										|| (p.BindingInfo?.BindingSource == null &&
											!p.ParameterType.IsPrimitive &&
											p.ParameterType != typeof(string) &&
											!p.ParameterType.IsEnum));

				return bodyParameter?.ParameterType;
			}
		} catch {
			// ignored
		}

		return null;
	}

	private string SanitizeJsonString(string jsonString, Type? modelType = null) {
		if (string.IsNullOrWhiteSpace(jsonString))
			return jsonString;

		try {
			using var document = JsonDocument.Parse(jsonString);

			if (modelType != null) {
				var sanitized = SanitizeJsonElementWithType(document.RootElement, modelType);
				return JsonSerializer.Serialize(sanitized, _JsonOptions);
			} else {
				var sanitized = SanitizeJsonElement(document.RootElement);
				return JsonSerializer.Serialize(sanitized, _JsonOptions);
			}
		} catch {
			return jsonString;
		}
	}

	private object? SanitizeJsonElementWithType(JsonElement element, Type type) {
		if (element.ValueKind == JsonValueKind.Object) {
			return SanitizeJsonObjectWithType(element, type);
		}

		if (element.ValueKind == JsonValueKind.Array) {
			if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>) ||
										type.GetGenericTypeDefinition() == typeof(IList<>) ||
										type.GetGenericTypeDefinition() == typeof(ICollection<>))) {
				var itemType = type.GetGenericArguments()[0];
				return element.EnumerateArray().Select(item => SanitizeJsonElementWithType(item, itemType)).ToArray();
			}

			return element.EnumerateArray().Select(SanitizeJsonElement).ToArray();
		}

		return SanitizeJsonElement(element);
	}

	private Dictionary<string, object?> SanitizeJsonObjectWithType(JsonElement obj, Type type) {
		var result = new Dictionary<string, object?>();
		var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

		foreach (var jsonProperty in obj.EnumerateObject()) {
			var propertyName = jsonProperty.Name;
			var camelCasePropertyName = char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
			var pascalCasePropertyName = char.ToUpperInvariant(propertyName[0]) + propertyName[1..];

			PropertyInfo? matchingProperty = null;
			if (properties.TryGetValue(propertyName, out var prop1)) matchingProperty = prop1;
			else if (properties.TryGetValue(camelCasePropertyName, out var prop2)) matchingProperty = prop2;
			else if (properties.TryGetValue(pascalCasePropertyName, out var prop3)) matchingProperty = prop3;

			if (matchingProperty?.GetCustomAttribute<IsSensitiveInformationAttribute>() != null) {
				result[propertyName] = SENSITIVE_INFORMATION;
			} else if (matchingProperty != null &&
						!matchingProperty.PropertyType.IsValueType &&
						matchingProperty.PropertyType != typeof(string)) {
				result[propertyName] = SanitizeJsonElementWithType(jsonProperty.Value, matchingProperty.PropertyType);
			} else {
				result[propertyName] = SanitizeJsonElement(jsonProperty.Value);
			}
		}

		return result;
	}

	private object? SanitizeJsonElement(JsonElement element) {
		return element.ValueKind switch {
			JsonValueKind.Object => SanitizeJsonObject(element),
			JsonValueKind.Array => element.EnumerateArray().Select(SanitizeJsonElement).ToArray(),
			JsonValueKind.String => element.GetString(),
			JsonValueKind.Number => element.GetDecimal(),
			JsonValueKind.True => true,
			JsonValueKind.False => false,
			_ => null,
		};
	}

	private Dictionary<string, object?> SanitizeJsonObject(JsonElement obj) {
		var result = new Dictionary<string, object?>();

		foreach (var property in obj.EnumerateObject()) {
			var propertyName = property.Name;
			result[propertyName] = SanitizeJsonElement(property.Value);
		}

		return result;
	}
}