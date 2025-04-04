using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Admin.Models.DTOs
{
    public class RoomDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Guid TreatmentMethodId { get; set; }
        public string TreatmentName { get; set; }
    }
}
