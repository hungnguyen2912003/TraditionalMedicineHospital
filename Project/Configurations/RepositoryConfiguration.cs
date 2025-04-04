using Project.Repositories;
using Project.Repositories.Implementations;
using Project.Repositories.Interfaces;

namespace Project.Configurations
{
    public static class RepositoryConfiguration
    {
        public static WebApplicationBuilder ConfigureRepository(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            builder.Services.AddScoped<IMedicineCategoryRepository, MedicineCategoryRepository>();
            builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();

            builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();

            builder.Services.AddScoped<IEmployeeCategoryRepository, EmployeeCategoryRepository>();
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            builder.Services.AddScoped<ITreatmentMethodRepository, TreatmentMethodRepository>();
            builder.Services.AddScoped<IRoomRepository, RoomRepository>();

            builder.Services.AddScoped<IRegulationRepository, RegulationRepository>();

            return builder;
        }
    }
}
