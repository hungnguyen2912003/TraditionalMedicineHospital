using Project.Models.Enums;

namespace Project.Areas.YTa.Models.DTOs
{
    public class UpdateTrackingDto
    {
        public TrackingStatus Status { get; set; }
        public string? Note { get; set; }
        public Guid EmployeeId { get; set; }
    }
}
