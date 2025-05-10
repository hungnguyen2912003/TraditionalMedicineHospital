namespace Project.Configurations
{
    public static class RoutingConfiguration
    {
        public static WebApplication ConfigureRouting(this WebApplication app)
        {
            // Route cụ thể cho logout
            app.MapControllerRoute(
                name: "logout",
                pattern: "logout",
                defaults: new { area = "Admin", controller = "Account", action = "Logout" }
            );

            // Route cụ thể cho change-password
            app.MapControllerRoute(
                name: "change-password",
                pattern: "change-password",
                defaults: new { area = "Admin", controller = "Account", action = "ChangePassword" }
            );

            // Route cụ thể cho login
            app.MapControllerRoute(
                name: "login",
                pattern: "login",
                defaults: new { area = "Admin", controller = "Account", action = "Login" }
            );

            app.MapControllerRoute(
                name: "access-denied",
                pattern: "access-denied",
                defaults: new { area = "Admin", controller = "Account", action = "AccessDenied" }
            );

            // Route cụ thể cho admin
            app.MapControllerRoute(
                name: "admin",
                pattern: "admin",
                defaults: new { area = "Admin", controller = "Home", action = "Index" }
            );

            // Route cụ thể cho staff
            app.MapControllerRoute(
                name: "staff",
                pattern: "staff",
                defaults: new { area = "Staff", controller = "Home", action = "Index" }
            );

            app.MapControllerRoute(
                name: "forgot-password",
                pattern: "forgot-password",
                defaults: new { area = "Admin", controller = "Account", action = "ForgotPassword" }
            );

            // Route cụ thể cho home
            app.MapControllerRoute(
                name: "home",
                pattern: "home",
                defaults: new { area = "", controller = "Home", action = "Index" }
            );
            
            // Route chung cho các area
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );

            // Route mặc định (không thuộc area)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );



            return app;
        }
    }
}
