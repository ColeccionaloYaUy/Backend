using API.API_Clean_Architecture.Filters;
using Microsoft.OpenApi;

namespace API.API_Clean_Architecture.Configurations.Builder;

public static class BuilderSwaggerConfig {
    public static void ConfigureSwagger(this IHostApplicationBuilder builder) {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c => {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            c.OperationFilter<FileUploadOperationFilter>();

            c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "JWT Authorization header using Bearer scheme",
            });

            c.AddSecurityRequirement(document => new() {
                [new("bearer", document)] = []
            });
        });

    }
}
