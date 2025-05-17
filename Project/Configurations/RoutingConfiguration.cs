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

            // Route cụ thể cho change-password
            app.MapControllerRoute(
                name: "doi-mat-khau",
                pattern: "doi-mat-khau",
                defaults: new { area = "Admin", controller = "Account", action = "ChangePassword" }
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

            app.MapControllerRoute(
                name: "quen-mat-khau",
                pattern: "quen-mat-khau",
                defaults: new { area = "Admin", controller = "Account", action = "ForgotPassword" }
            );

            // Route cụ thể cho admin
            app.MapControllerRoute(
                name: "admin",
                pattern: "admin",
                defaults: new { area = "Admin", controller = "Home", action = "Index" }
            );

            app.MapControllerRoute(
                name: "danh-muc-nguoi-dung",
                pattern: "danh-muc-nguoi-dung",
                defaults: new { area = "Admin", controller = "Users", action = "Index" }
            );


            /////////////////////////////////////////////
            // Danh mục loại thuốc
            /////////////////////////////////////////////
            app.MapControllerRoute(
                name: "danh-muc-loai-thuoc",
                pattern: "danh-muc-loai-thuoc",
                defaults: new { area = "Admin", controller = "MedicineCategories", action = "Index" }
            );

            app.MapControllerRoute(
                name: "tao-moi-loai-thuoc",
                pattern: "tao-moi-loai-thuoc",
                defaults: new { area = "Admin", controller = "MedicineCategories", action = "Create" }
            );

            app.MapControllerRoute(
                name: "sua-loai-thuoc",
                pattern: "sua-loai-thuoc/{id}",
                defaults: new { area = "Admin", controller = "MedicineCategories", action = "Edit" }
            );

            app.MapControllerRoute(
                name: "xem-chi-tiet-loai-thuoc",
                pattern: "xem-chi-tiet-loai-thuoc/{id}",
                defaults: new { area = "Admin", controller = "MedicineCategories", action = "Details" }
            );

            app.MapControllerRoute(
                name: "thung-rac-loai-thuoc",
                pattern: "thung-rac-loai-thuoc",
                defaults: new { area = "Admin", controller = "MedicineCategories", action = "Trash" }
            );
                  
            app.MapControllerRoute(
                name: "danh-muc-thuoc",
                pattern: "danh-muc-thuoc",
                defaults: new { area = "Admin", controller = "Medicines", action = "Index" }
            );

            app.MapControllerRoute(
                name: "danh-muc-phuong-phap-dieu-tri",
                pattern: "danh-muc-phuong-phap-dieu-tri",
                defaults: new { area = "Admin", controller = "TreatmentMethods", action = "Index" }
            );

            app.MapControllerRoute(
                name: "danh-muc-phong",
                pattern: "danh-muc-phong",
                defaults: new { area = "Admin", controller = "Rooms", action = "Index" }
            );

            app.MapControllerRoute(
                name: "danh-muc-quy-dinh",
                pattern: "danh-muc-quy-dinh",
                defaults: new { area = "Admin", controller = "Regulations", action = "Index" }
            );

            app.MapControllerRoute(
                name: "danh-muc-khoa",
                pattern: "danh-muc-khoa",
                defaults: new { area = "Admin", controller = "Departments", action = "Index" }
            );

            app.MapControllerRoute(
                name: "danh-muc-loai-nhan-su",
                pattern: "danh-muc-loai-nhan-su",
                defaults: new { area = "Admin", controller = "EmployeeCategories", action = "Index" }
            );

            app.MapControllerRoute(
                name: "danh-muc-nhan-su",
                pattern: "danh-muc-nhan-su",
                defaults: new { area = "Admin", controller = "Employees", action = "Index" }
            );

            // Route cụ thể cho staff
            app.MapControllerRoute(
                name: "nhan-vien",
                pattern: "nhan-vien",
                defaults: new { area = "Staff", controller = "Home", action = "Index" }
            );

            app.MapControllerRoute(
                name: "danh-muc-benh-nhan",
                pattern: "danh-muc-benh-nhan",
                defaults: new { area = "Staff", controller = "Patients", action = "Index" }
            );

            app.MapControllerRoute(
                name: "phieu-kham",
                pattern: "phieu-kham",
                defaults: new { area = "Staff", controller = "TreatmentRecords", action = "Index" }
            );

            app.MapControllerRoute(
                name: "danh-muc-bao-hiem-y-te",
                pattern: "danh-muc-bao-hiem-y-te",
                defaults: new { area = "Staff", controller = "HealthInsurances", action = "Index" }
            );

            app.MapControllerRoute(
                name: "don-thuoc",
                pattern: "don-thuoc",
                defaults: new { area = "Staff", controller = "Prescriptions", action = "Index" }
            );

            app.MapControllerRoute(
                name: "thanh-toan",
                pattern: "thanh-toan",
                defaults: new { area = "Staff", controller = "Payments", action = "Index" }
            );
            
            app.MapControllerRoute(
                name: "thong-ke",
                pattern: "thong-ke",
                defaults: new { area = "Staff", controller = "Statistics", action = "Index" }
            );

            app.MapControllerRoute(
                name: "canh-bao-benh-nhan",
                pattern: "canh-bao-benh-nhan",
                defaults: new { area = "Staff", controller = "WarningPatients", action = "Index" }
            );
            
            app.MapControllerRoute(
                name: "tiep-nhan-benh-nhan",
                pattern: "tiep-nhan-benh-nhan",
                defaults: new { area = "Staff", controller = "Receptions", action = "Create" }
            );

            app.MapControllerRoute(
                name: "theo-doi-benh-nhan",
                pattern: "theo-doi-benh-nhan",
                defaults: new { area = "Staff", controller = "TreatmentTrackings", action = "Index" }
            );

            // Route cụ thể cho bệnh nhân
            app.MapControllerRoute(
                name: "benh-nhan",
                pattern: "benh-nhan",
                defaults: new { area = "BenhNhan", controller = "Home", action = "Index" }
            );

            // Route cụ thể cho home
            app.MapControllerRoute(
                name: "trang-chu",
                pattern: "trang-chu",
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
