using Project.Models.Enums;

namespace Project.Areas.YTa.Models.DTOs
{
    public class TreatmentTrackingCreateDto
    {
        public Guid PatientId { get; set; }
        public Guid RoomId { get; set; }
        public string? Note { get; set; }
        public TrackingStatus Status { get; set; }
        public DateTime TrackingDate { get; set; }
        public Guid EmployeeId { get; set; }
    }
}
