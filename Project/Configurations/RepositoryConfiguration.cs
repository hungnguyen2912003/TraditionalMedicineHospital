using Project.Repositories;
using Project.Repositories.Implementations;
using Project.Repositories.Interfaces;
using Repositories.Implementations;
using Repositories.Interfaces;

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

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<ITreatmentRecordRepository, TreatmentRecordRepository>();
            builder.Services.AddScoped<IHealthInsuranceRepository, HealthInsuranceRepository>();
            builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();

            builder.Services.AddScoped<ITreatmentRecordDetailRepository, TreatmentRecordDetailRepository>();
            builder.Services.AddScoped<ITreatmentRecordRegulationRepository, TreatmentRecordRegulationRepository>();

            builder.Services.AddScoped<ITreatmentTrackingRepository, TreatmentTrackingRepository>();

            builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
            builder.Services.AddScoped<IPrescriptionDetailRepository, PrescriptionDetailRepository>();

            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

            builder.Services.AddScoped<IWarningSentRepository, WarningSentRepository>();

            return builder;
        }
    }
}
