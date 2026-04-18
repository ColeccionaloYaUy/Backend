using API.API_Clean_Architecture.Middlewares;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace API.API_Clean_Architecture.Configurations.Application;

public static class ApplicationConfiguration {
    public static WebApplication Configure(this WebApplication app) {
        app.UseHsts();
        app.UseHttpsRedirection();

        app.UseCors(ConfigurationConstants.MY_CORS);

        app.UseResponseCompression();
        app.UseRateLimiter();

        //app.UseMiddleware<SecurityHeadersMiddleware>();

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseMiddleware<DbConnectionMiddleware>();

        //app.UseMiddleware<LoggingMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            c.RoutePrefix = "api/docs";
            c.DocExpansion(DocExpansion.None);
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("api/health");

        app.MapControllers();

        return app;
    }
}