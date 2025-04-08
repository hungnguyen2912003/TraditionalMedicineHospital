namespace Project.Areas.Admin.Models.ViewModels
{
    public class RoomViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TreatmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
