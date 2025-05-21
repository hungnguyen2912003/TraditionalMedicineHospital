namespace Project.Configurations
{
    public static class RoutingConfiguration
    {
        public static WebApplication ConfigureRouting(this WebApplication app)
        {
            // Route cụ thể cho logout
            app.MapControllerRoute(
                name: "dang-xuat",
                pattern: "dang-xuat",
                defaults: new { area = "Admin", controller = "Account", action = "Logout" }
            );

            app.MapControllerRoute(
                name: "quen-mat-khau",
                pattern: "quen-mat-khau",
                defaults: new { area = "Admin", controller = "Account", action = "ForgotPassword" }
            );

            // Route cụ thể cho login
            app.MapControllerRoute(
                name: "dang-nhap",
                pattern: "dang-nhap",
                defaults: new { area = "Admin", controller = "Account", action = "Login" }
            );

            app.MapControllerRoute(
                name: "tu-choi-truy-cap",
                pattern: "tu-choi-truy-cap",
                defaults: new { area = "Admin", controller = "Account", action = "AccessDenied" }
            );            

            // Route cụ thể cho home
            app.MapControllerRoute(
                name: "trang-chu",
                pattern: "trang-chu",
                defaults: new { area = "", controller = "Home", action = "Index" }
            );

            app.MapControllerRoute(
                name: "admin",
                pattern: "admin",
                defaults: new { area = "Admin", controller = "Home", action = "Index" }
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
