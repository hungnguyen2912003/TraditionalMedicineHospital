using Project.Areas.Admin.Models.Entities;
using Project.Models.Enums;

namespace Project.Areas.Staff.Models.ViewModels
{
    public class PaymentViewModel
    {
        public Guid Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Code { get; set; } = string.Empty;
        public string TreatmentRecordCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public decimal TotalPrescriptionCost { get; set; }
        public decimal TotalTreatmentMethodCost { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal TotalCost { get; set; }
        public bool IsActive { get; set; }
        public PaymentStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public decimal AdvancePayment { get; set; }
        public decimal FinalCost { get; set; }
        public List<Prescription> Prescriptions { get; set; } = new();
        public string? PatientHealthInsuranceNumber { get; set; }
        public DateTime? PatientHealthInsuranceExpiredDate { get; set; }
        public HealthInsuranceRegistrationPlace? PatientHealthInsurancePlaceOfRegistration { get; set; }
        public bool? PatientHealthInsuranceIsRightRoute { get; set; }
        public List<TreatmentRecordDetail> TreatmentDetails { get; set; } = new();
    }
}
