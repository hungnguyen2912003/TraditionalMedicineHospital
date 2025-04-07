using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Project.Areas.Admin.Models.Entities;
using Project.Helpers;
using Project.Services;
using Project.Services.Implementations;
using Project.Services.Interfaces;
using System.Text;

namespace Project.Configurations
{
    public static class ServiceConfiguration
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
            builder.Services.AddScoped<IBaseService<Medicine>, MedicineService>();
            builder.Services.AddScoped<IBaseService<MedicineCategory>, MedicineCategoryService>();
            builder.Services.AddScoped<IBaseService<Department>, DepartmentService>();
            builder.Services.AddScoped<IBaseService<EmployeeCategory>, EmployeeCategoryService>();
            builder.Services.AddScoped<IBaseService<Employee>, EmployeeService>();
            builder.Services.AddScoped<IBaseService<Room>, RoomService>();
            builder.Services.AddScoped<IBaseService<Regulation>, RegulationService>();

            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<JwtManager>();
            builder.Services.AddControllersWithViews();


            builder.Services.AddAuthorization();
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();
            return builder;
        }
    }
}
