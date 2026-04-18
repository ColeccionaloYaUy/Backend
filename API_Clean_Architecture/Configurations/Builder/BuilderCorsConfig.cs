namespace API.API_Clean_Architecture.Configurations.Builder;

public static class BuilderCorsConfig {
	public static string PolicyName => ConfigurationConstants.MY_CORS;

	public static void ConfigureCors(this IHostApplicationBuilder builder) {
		builder.Services.AddCors(options => {
			options.AddPolicy(ConfigurationConstants.MY_CORS, policy =>
				policy.AllowAnyOrigin()
					.AllowAnyHeader()
					.AllowAnyMethod());
		});
	}
}