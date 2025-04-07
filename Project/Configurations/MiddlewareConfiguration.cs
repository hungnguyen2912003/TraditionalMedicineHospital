namespace Project.Configurations
{
    public static class MiddlewareConfiguration
    {
        public static WebApplication ConfigureMiddleware(this WebApplication app)
        {
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
