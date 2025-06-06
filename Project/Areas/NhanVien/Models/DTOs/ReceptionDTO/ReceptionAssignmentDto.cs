using System;

namespace Project.Areas.Staff.Models.DTOs.ReceptionDTO
{
    public class ReceptionAssignmentDto
    {
        // Assignment info
        public string Code { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Note { get; set; }
        public string? CreatedBy { get; set; }
    }
}
