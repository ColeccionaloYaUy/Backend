using API.API_Clean_Architecture.Filters;

namespace API.API_Clean_Architecture.Configurations.Builder;

public static class BuilderControllersConfig {
	public static void ConfigureControllers(this IHostApplicationBuilder builder) {
		builder.Services.AddControllers(options => {
			options.Filters.Add<ModelStateValidationFilter>();
			options.Filters.Add<ResponseFilter>();
		});
	}
}