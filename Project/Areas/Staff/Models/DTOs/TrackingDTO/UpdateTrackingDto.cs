using Project.Models.Enums;

namespace Hospital.Areas.Staff.Models.DTOs.TrackingDTO
{
    public class UpdateTrackingDto
    {
        public TrackingStatus Status { get; set; }
        public string? Note { get; set; }
        public Guid EmployeeId { get; set; }
    }
}


