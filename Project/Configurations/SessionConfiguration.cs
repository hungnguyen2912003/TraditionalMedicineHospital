namespace Project.Configurations
{
    public static class SessionConfiguration
    {
        public static WebApplicationBuilder ConfigureSession(this WebApplicationBuilder builder)
        {
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            return builder;
        }
    }
}
