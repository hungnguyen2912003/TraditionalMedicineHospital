namespace Project.Configurations
{
    public static class ServiceConfiguration
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllersWithViews();
            return builder;
        }
    }
}
