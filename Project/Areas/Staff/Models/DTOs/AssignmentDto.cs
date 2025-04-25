using System;

namespace Project.Areas.Staff.Models.DTOs
{
    public class AssignmentDto
    {
        public string Code { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class AssignmentUpdateDto
    {
        public string Code { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
} 