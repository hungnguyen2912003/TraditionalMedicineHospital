using System.ComponentModel.DataAnnotations;
using Project.Models.Enums;

namespace Project.Areas.Admin.Models.DTOs
{
    public class RoomDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? TreatmentMethodId { get; set; }
        public string? TreatmentMethodName { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
    }
}
