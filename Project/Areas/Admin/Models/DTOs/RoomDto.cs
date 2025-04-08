using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Admin.Models.DTOs
{
    public class RoomDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid TreatmentMethodId { get; set; }
        public string TreatmentName { get; set; } = string.Empty;
    }
}
