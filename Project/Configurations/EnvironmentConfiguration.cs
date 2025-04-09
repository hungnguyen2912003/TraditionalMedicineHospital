namespace Project.Configurations
{
    public static class EnvironmentConfiguration
    {
        public static WebApplication ConfigureEnvironment(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            return app;
        }
    }
}