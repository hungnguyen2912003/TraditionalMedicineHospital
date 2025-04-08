namespace Project.Areas.Admin.Models.ViewModels
{
    public class TreatmentMethodViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string DepName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
