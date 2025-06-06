using System;

namespace Project.Areas.BacSi.Models.DTOs
{
    public class AssignmentDto
    {
        public string Code { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Note { get; set; }
        public string? CreatedBy { get; set; }
        public string DoctorName { get; set; } = string.Empty;
    }

    public class AssignmentUpdateDto
    {
        public string Code { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Note { get; set; }
    }
}