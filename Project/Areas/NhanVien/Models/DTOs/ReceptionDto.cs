﻿using Project.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Project.Areas.NhanVien.Models.DTOs
{
    public class ReceptionDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public GenderType Gender { get; set; }
        public string? IdentityNumber { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        [EmailAddress]
        public string? EmailAddress { get; set; }
        public IFormFile? ImageFile { get; set; }

        // Optional Health insurance
        public bool HasHealthInsurance { get; set; }

        // Health insurance info
        public string? HealthInsuranceCode { get; set; }
        public string? HealthInsuranceNumber { get; set; }
        public DateTime? HealthInsuranceExpiryDate { get; set; }
        public HealthInsuranceRegistrationPlace? HealthInsurancePlaceOfRegistration { get; set; }
        public bool? HealthInsuranceIsRightRoute { get; set; }
    }
}
