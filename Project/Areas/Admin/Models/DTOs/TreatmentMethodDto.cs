﻿namespace Project.Areas.Admin.Models.DTOs
{
    public class TreatmentMethodDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Cost { get; set; }
    }
}
