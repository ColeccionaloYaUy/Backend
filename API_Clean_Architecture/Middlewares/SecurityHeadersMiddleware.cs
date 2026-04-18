namespace API.API_Clean_Architecture.Middlewares;

public class SecurityHeadersMiddleware {
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context) {
        var isDevelopment = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
        if (!isDevelopment) {
            // Strict Transport Security
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");


            // X-Frame-Options
            context.Response.Headers.Append("X-Frame-Options", "DENY");

            // X-Content-Type-Options
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            // Referrer Policy
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            // X-XSS-Protection
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

            // Permissions Policy
            context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), location=(), payment=()");

            // Content Security Policy
            context.Response.Headers.Append("Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data:; " +
                "font-src 'self'; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none';");
            // Remove Server header
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
        }


        await _next(context);
    }
}