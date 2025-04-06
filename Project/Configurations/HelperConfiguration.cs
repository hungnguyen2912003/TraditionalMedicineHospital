using Project.Helpers;

namespace Project.Configurations
{
    public static class HelperConfiguration
    {
        public static WebApplicationBuilder ConfigureHelpers(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<CodeGeneratorHelper>();
            builder.Services.AddScoped<ViewBagHelper>();
            return builder;
        }
    }
}
