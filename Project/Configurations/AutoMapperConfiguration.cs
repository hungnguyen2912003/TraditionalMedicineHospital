using Project.Mappers;

namespace Project.Configurations
{
    public static class AutoMapperConfiguration
    {
        public static WebApplicationBuilder ConfigureMappers(this WebApplicationBuilder builder)
        {
            builder.Services.AddAutoMapper(typeof(MedicineCategoryProfile));

            return builder;
        }
    }
}
