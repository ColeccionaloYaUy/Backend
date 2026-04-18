using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.API_Clean_Architecture.Configurations.Builder;

public static class BuilderJsonConfig {
	public static void ConfigureJson(this IHostApplicationBuilder builder) {
		builder.Services.ConfigureHttpJsonOptions(options => {
			options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
			options.SerializerOptions.WriteIndented = false;
		});
	}
}