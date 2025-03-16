using FluentValidation;
using Project.Validators;

namespace Project.Configurations
{
    public static class ValidatorConfiguration
    {
        public static WebApplicationBuilder ConfigureValidator(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ImageValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<MedicineCategoryValidator>();

            return builder;
        }
    }
}