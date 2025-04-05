using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Project.Services;
using System.Text;

namespace Project.Configurations
{
    public static class JWTConfiguration
    {
        public static WebApplicationBuilder ConfigureJWT(this WebApplicationBuilder builder)
        {
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
                    ValidIssuer = "http://localhost:5285/",
                    ValidAudience = "TraditionalMedicineHospital",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MCKxk26zBjrUSuD94ZeQ3Ww5Vbmf8sLdUq4ZFywJrujQp3CPXbYeA72KVL5fTGEMpyCkBEPKeXDRZbqF7TjwAnVLuxmGQcUSRYF7BjEQxJn2vkHTMSPgpL9VAcmWthzeFrjyvL4ext6CYVNhW2sfMXkQg9nARdKb"))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["AuthToken"];
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddScoped<JwtManager>();

            return builder;
        }
    }
}
