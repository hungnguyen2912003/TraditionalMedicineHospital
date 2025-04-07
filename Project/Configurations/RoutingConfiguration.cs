namespace Project.Configurations
{
    public static class RoutingConfiguration
    {
        public static WebApplication ConfigureRouting(this WebApplication app)
        {
            app.MapControllerRoute(
                name: "logout",
                pattern: "logout",
                defaults: new { area = "Admin", controller = "Account", action = "Logout" }
            );

            app.MapControllerRoute(
                name: "change-password",
                pattern: "change-password",
                defaults: new { area = "Admin", controller = "Account", action = "ChangePassword" }
            );

            app.MapControllerRoute(
                name: "login",
                pattern: "login",
                defaults: new { area = "Admin", controller = "Account", action = "Login" }
            );

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
