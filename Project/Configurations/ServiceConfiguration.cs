using Project.Areas.Admin.Models.Entities;
using Project.Areas.Staff.Models.Entities;
using Project.Services;
using Project.Services.Features;
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
            builder.Services.AddScoped<IBaseService<Medicine>, MedicineService>();
            builder.Services.AddScoped<IBaseService<MedicineCategory>, MedicineCategoryService>();
            builder.Services.AddScoped<IBaseService<Department>, DepartmentService>();
            builder.Services.AddScoped<IBaseService<EmployeeCategory>, EmployeeCategoryService>();
            builder.Services.AddScoped<IBaseService<Employee>, EmployeeService>();
            builder.Services.AddScoped<IBaseService<Room>, RoomService>();
            builder.Services.AddScoped<IBaseService<Regulation>, RegulationService>();
            builder.Services.AddScoped<IBaseService<Patient>, PatientService>();
            builder.Services.AddScoped<IBaseService<TreatmentRecord>, TreatmentRecordService>();

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
