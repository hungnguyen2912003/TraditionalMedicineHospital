using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;

namespace Project.Areas.Admin.Data
{
    public class TraditionalMedicineHospitalDbContext : DbContext
    {
        public TraditionalMedicineHospitalDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<MedicineCategory> medicineCategories { get; set; }
        public DbSet<Medicine> medicines { get; set; }
        public DbSet<Department> departments { get; set; }
        public DbSet<EmployeeCategory> employeeCategories { get; set; }
        public DbSet<Employee> employees { get; set; }
        public DbSet<TreatmentMethod> treatments { get; set; }
        public DbSet<Room> rooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Medicine>()
                .Property(e => e.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TreatmentMethod>()
                .Property(e => e.Cost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Medicine>()
                .HasOne(m => m.MedicineCategory)
                .WithMany(mc => mc.Medicines)
                .HasForeignKey(m => m.MedicineCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
