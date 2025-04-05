using Project.Mappers;
using Project.Validators;

namespace Project.Configurations
{
    public static class ValidatorConfiguration
    {
        public static WebApplicationBuilder ConfigureValidator(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<AuthValidator>();

            return builder;
        }
    }
}
