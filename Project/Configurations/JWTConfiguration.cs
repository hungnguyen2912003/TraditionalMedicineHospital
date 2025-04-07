using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Project.Configurations
{
    public static class JWTConfiguration
    {
        public static WebApplicationBuilder ConfigureJWT(this WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration;

            var issuer = configuration["JwtSettings:Issuer"];
            var audience = configuration["JwtSettings:Audience"];
            var secretKey = configuration["JwtSettings:SecretKey"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["AuthToken"];
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.Response.Redirect("/login");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            });

            return builder;
        }
    }
}