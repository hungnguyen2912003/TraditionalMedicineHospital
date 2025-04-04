using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Admin.Models.DTOs
{
    public class RegulationDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string? Description { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
