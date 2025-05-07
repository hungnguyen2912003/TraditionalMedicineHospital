using Project.Models.Enums;

namespace Project.Areas.Staff.Models.ViewModels
{
    public class TreatmentTrackingViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime TrackingDate { get; set; }
        public string? Note { get; set; }
        public TrackingStatus Status { get; set; }
        public Guid TreatmentRecordDetailId { get; set; }
        public bool IsActive { get; set; }
        public string? PatientName { get; set; }
        public string? RoomName { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorCode { get; set; }
    }
}


