﻿using Project.Models.Enums;

namespace Project.Areas.Admin.Models.DTOs
{
    public class EmployeeDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
        public GenderType Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public EmployeeStatus Status { get; set; }
        public Guid EmployeeCategoryId { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid RoomId { get; set; }
        public Guid TreatmentMethodId { get; set; }
        public bool IsActive { get; set; }
    }
}
