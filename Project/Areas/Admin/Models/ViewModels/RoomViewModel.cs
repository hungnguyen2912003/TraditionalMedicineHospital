namespace Project.Areas.Admin.Models.ViewModels
{
    public class RoomViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string TreatmentName { get; set; }
        public bool IsActive { get; set; }
    }
}
