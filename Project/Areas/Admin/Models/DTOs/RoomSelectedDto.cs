namespace Project.Areas.Admin.Models.DTOs
{
    public class RoomSelectedDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
} 