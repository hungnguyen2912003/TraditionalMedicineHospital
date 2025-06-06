using Project.Areas.Admin.Models.Entities;
using Project.Models.Enums;

namespace Project.Areas.BacSi.Models.ViewModels
{
    public class TreatmentRecordViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TreatmentStatus Status { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
