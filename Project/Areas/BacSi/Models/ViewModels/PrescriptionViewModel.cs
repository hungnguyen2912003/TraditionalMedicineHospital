using Project.Models.Enums;

namespace Project.Areas.BacSi.Models.ViewModels
{
    public class PrescriptionDetailViewModel
    {
        public Guid MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
    }

    public class AssignmentViewModel
    {
        public string CreatedBy { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
    }

    public class PrescriptionViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime PrescriptionDate { get; set; }
        public decimal TotalPrice => PrescriptionDetails?.Sum(d => (d.Price ?? 0) * d.Quantity) ?? 0;
        public string? Note { get; set; }
        public Guid TreatmentRecordId { get; set; }
        public string TreatmentRecordCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public TreatmentStatus TreatmentRecordStatus { get; set; }
        public List<PrescriptionDetailViewModel> PrescriptionDetails { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public string? UpdatedByName { get; set; }
        public List<AssignmentViewModel> Assignments { get; set; } = new();
    }

    public class MedicineViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
