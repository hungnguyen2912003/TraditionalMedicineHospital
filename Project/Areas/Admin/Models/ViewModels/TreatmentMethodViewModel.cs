namespace Project.Areas.Admin.Models.ViewModels
{
    public class TreatmentMethodViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string DepName { get; set; }
        public bool IsActive { get; set; }
    }
}
