using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace API.API_Clean_Architecture.Configurations.Builder;

public static class BuilderAuthConfig {
    public static void ConfigureAuth(this IHostApplicationBuilder builder) {
        var secret = builder.Configuration["Auth:Secret"] ?? throw new Exception("Missing Secret");
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        builder.Services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt => {
            opt.RequireHttpsMetadata = false;

            opt.SaveToken = true;
            opt.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretBytes),

                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Auth:ValidIssuer"],

                ValidateAudience = true,
                ValidAudience = builder.Configuration["Auth:ValidAudience"],

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Email,
            };
            opt.IncludeErrorDetails = true;
            opt.Events = new JwtBearerEvents {
                OnMessageReceived = context => {
                    var header = context.Request.Headers.Authorization.ToString();
                    Log.Information("JWT header received: {Header}",
                        string.IsNullOrEmpty(header) ? "[MISSING]" : header[..Math.Min(40, header.Length)] + "...");
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context => {
                    Log.Warning("JWT authentication failed: Type={Type}, Message={Message}, Inner={Inner}",
                        context.Exception.GetType().Name,
                        context.Exception.Message,
                        context.Exception.InnerException?.Message);
                    return Task.CompletedTask;
                },
                OnChallenge = context => {
                    Log.Warning("JWT challenge: error={Error}, description={Description}",
                        context.Error, context.ErrorDescription);
                    return Task.CompletedTask;
                },
            };
        });
        builder.Services.AddAuthorization();
    }

}