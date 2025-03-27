using Project.Services.Implementations;
using Project.Services.Interfaces;

namespace Project.Configurations
{
    public static class ServiceConfiguration
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IImageService, ImageService>();

            builder.Services.AddControllersWithViews();
            return builder;
        }
    }
}
