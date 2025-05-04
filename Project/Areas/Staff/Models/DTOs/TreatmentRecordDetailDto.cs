using System;

namespace Project.Areas.Staff.Models.DTOs
{
    public class TreatmentRecordDetailDto
    {
        public string Code { get; set; } = string.Empty;
        public Guid RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public Guid TreatmentMethodId { get; set; }
        public Guid TreatmentRecordId { get; set; }
        public string TreatmentMethodName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
    }

    public class TreatmentRecordDetailUpdateDto
    {
        public string Code { get; set; } = string.Empty;
        public Guid RoomId { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}