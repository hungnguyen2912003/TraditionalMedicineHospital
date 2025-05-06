using Project.Models.Enums;

namespace Project.Areas.Staff.Models.ViewModels
{
    public class TreatmentTrackingViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime TrackingDate { get; set; }
        public TrackingStatus Status { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}


