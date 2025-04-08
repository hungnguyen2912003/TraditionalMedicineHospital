using Project.Mappers;

namespace Project.Configurations
{
    public static class AutoMapperConfiguration
    {
        public static WebApplicationBuilder ConfigureMappers(this WebApplicationBuilder builder)
        {
            builder.Services.AddAutoMapper(typeof(MedicineCategoryProfile));
            builder.Services.AddAutoMapper(typeof(MedicineProfile));
            builder.Services.AddAutoMapper(typeof(DepartmentProfile));
            builder.Services.AddAutoMapper(typeof(EmployeeCategoryProfile));
            builder.Services.AddAutoMapper(typeof(EmployeeProfile));
            builder.Services.AddAutoMapper(typeof(TreatmentMethodProfile));
            builder.Services.AddAutoMapper(typeof(RoomProfile));
            builder.Services.AddAutoMapper(typeof(RegulationProfile));
            builder.Services.AddAutoMapper(typeof(PatientProfile));
            builder.Services.AddAutoMapper(typeof(TreatmentRecordProfile));

            return builder;
        }
    }
}
