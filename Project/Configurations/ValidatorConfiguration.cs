using FluentValidation;
using Project.Areas.Admin.Models.DTOs;
using Project.Validators;
using Project.Validators.Commons;

namespace Project.Configurations
{
    public static class ValidatorConfiguration
    {
        public static WebApplicationBuilder ConfigureValidator(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IValidator<MedicineCategoryDto>, MedicineCategoryValidator>();
            return builder;
        }
    }
}