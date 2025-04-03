using Project.Areas.Admin.Models.Entities;
using Project.Helpers;
using Project.Services;
using Project.Services.Implementations;
using Project.Services.Interfaces;

namespace Project.Configurations
{
    public static class ServiceConfiguration
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
            builder.Services.AddScoped<CodeGeneratorHelper>();
            builder.Services.AddScoped<IBaseService<Medicine>, MedicineService>();
            builder.Services.AddScoped<IBaseService<MedicineCategory>, MedicineCategoryService>();
            builder.Services.AddScoped<IBaseService<Department>, DepartmentService>();
            builder.Services.AddScoped<IBaseService<EmployeeCategory>, EmployeeCategoryService>();
            builder.Services.AddScoped<IBaseService<Employee>, EmployeeService>();

            builder.Services.AddControllersWithViews();
            return builder;
        }
    }
}
