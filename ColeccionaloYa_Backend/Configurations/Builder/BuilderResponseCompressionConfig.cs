using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace API.API_Clean_Architecture.Configurations.Builder;

public static class BuilderResponseCompressionConfig {
    public static void ConfigureResponseCompression(this IHostApplicationBuilder builder) {
        builder.Services.AddResponseCompression(options => {
            options.EnableForHttps = true;
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
        });

        builder.Services.Configure<GzipCompressionProviderOptions>(options => {
            options.Level = CompressionLevel.Fastest;
        });
    }
}