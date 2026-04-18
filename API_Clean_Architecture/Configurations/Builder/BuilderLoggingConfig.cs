using Serilog;

namespace API.API_Clean_Architecture.Configurations.Builder;

public static class BuilderLoggingConfig
{
    public static void ConfigureLogging(this IHostApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, "Logs", "log-.json"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}",
                shared: true,
                retainedFileCountLimit: 30
            )
            .CreateLogger();

        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);
    }
}