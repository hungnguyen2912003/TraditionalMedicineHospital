using System;

namespace Project.Areas.BacSi.Models.DTOs
{
    public class TreatmentRecordDetailDto
    {
        // Treatment record detail info
        public string Code { get; set; } = string.Empty;
        public Guid RoomId { get; set; }
        public string? Note { get; set; }
        public Guid TreatmentRecordId { get; set; }
        public Guid TreatmentMethodId { get; set; }
        public string? CreatedBy { get; set; }
        public string? EmployeeName { get; set; }
        public Guid EmployeeId { get; set; }

        // Navigation properties
        public string? TreatmentMethodName { get; set; }
        public string? RoomName { get; set; }
        public string? DoctorName { get; set; }
    }

    public class TreatmentRecordDetailUpdateDto
    {
        public string Code { get; set; } = string.Empty;
        public Guid RoomId { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}