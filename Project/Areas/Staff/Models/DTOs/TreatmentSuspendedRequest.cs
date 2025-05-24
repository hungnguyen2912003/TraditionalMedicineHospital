using Project.Models.Enums;

namespace Project.Areas.Staff.Models.DTOs
{
    public class TreatmentSuspendedRequest
    {
        public Guid TreatmentRecordId { get; set; }
        public string SuspendedReason { get; set; } = string.Empty;
        public string? SuspendedNote { get; set; } = string.Empty;
        public string SuspendedBy { get; set; } = string.Empty;
        public DateTime SuspendedDate { get; set; }
        public TreatmentStatus Status { get; set; }
    }
}
