﻿using Microsoft.EntityFrameworkCore;
using Project.Areas.Admin.Models.Entities;

namespace Project.Datas
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
        public DbSet<Regulation> regulations { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<Patient> patients { get; set; }
        public DbSet<TreatmentRecord> treatmentRecords { get; set; }
        public DbSet<Assignment> assignments { get; set; }
        public DbSet<Prescription> prescriptions { get; set; }
        public DbSet<PrescriptionDetail> prescriptionDetails { get; set; }
        public DbSet<TreatmentRecord_Regulation> treatmentRecord_Regulations { get; set; }
        public DbSet<Payment> payments { get; set; }
        public DbSet<TreatmentRecordDetail> treatmentRecordDetails { get; set; }
        public DbSet<TreatmentTracking> treatmentTrackings { get; set; }
        public DbSet<HealthInsurance> healthInsurances { get; set; }
        public DbSet<WarningSent> warningSents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Medicine>()
                .Property(e => e.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TreatmentMethod>()
                .Property(e => e.Cost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TreatmentRecord>()
                .Property(e => e.AdvancePayment)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Medicine>()
                .HasOne(m => m.MedicineCategory)
                .WithMany(mc => mc.Medicines)
                .HasForeignKey(m => m.MedicineCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<User>(u => u.EmployeeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Patient>()
                .HasOne(e => e.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<User>(u => u.PatientId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Patient>()
                .HasMany(p => p.TreatmentRecords)
                .WithOne(t => t.Patient)
                .HasForeignKey(t => t.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TreatmentRecordDetail>()
                .HasOne(d => d.TreatmentRecord)
                .WithMany(t => t.TreatmentRecordDetails)
                .HasForeignKey(d => d.TreatmentRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TreatmentRecord_Regulation>()
                .HasOne(tr => tr.TreatmentRecord)
                .WithMany(t => t.TreatmentRecord_Regulations)
                .HasForeignKey(tr => tr.TreatmentRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TreatmentTracking>()
                .HasOne(t => t.TreatmentRecordDetail)
                .WithMany(d => d.TreatmentTrackings)
                .HasForeignKey(t => t.TreatmentRecordDetailId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
