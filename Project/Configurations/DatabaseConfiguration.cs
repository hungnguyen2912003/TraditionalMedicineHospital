using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Data;

namespace Project.Configurations
{
    public static class DatabaseConfiguration
    {
        public static WebApplicationBuilder ConfigureDatabase(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<TraditionalMedicineHospitalDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            return builder;
        }
    }
}
