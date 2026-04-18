using API.Utils.DI;

namespace API.API_Clean_Architecture.Configurations.Builder;

public static class BuilderConfigure {
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder) {
        builder.ConfigureCors();
        builder.ConfigureJson();
        builder.ConfigureSwagger();
        builder.ConfigureResponseCompression();
        builder.ConfigureRateLimiter();

        builder.AddInjectables();

        builder.ConfigureMediatR();

        builder.ConfigureLogging();

        builder.ConfigureAuth();

        builder.ConfigureControllers();

        builder.Services.AddHealthChecks();

        return builder;
    }
}