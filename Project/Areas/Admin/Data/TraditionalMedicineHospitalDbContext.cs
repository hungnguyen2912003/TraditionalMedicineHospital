using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;

namespace Project.Areas.Admin.Data
{
    public class TraditionalMedicineHospitalDbContext : DbContext
    {
        public TraditionalMedicineHospitalDbContext(DbContextOptions options) : base(options)
        {
        }

        DbSet<MedicineCategory> medicineCategories { get; set; }
        DbSet<Medicine> medicines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Medicine>()
                .Property(e => e.Price)
                .HasPrecision(18, 2);
        }
    }
}
