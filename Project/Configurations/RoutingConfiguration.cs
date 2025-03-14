namespace Project.Configurations
{
    public static class RoutingConfiguration
    {
        public static WebApplication ConfigureRouting(this WebApplication app)
        {
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            return app;
        }
    }
}
