namespace Project.Areas.Admin.Models.DTOs
{
    public class TreatmentMethodDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public Guid DepartmentId { get; set; }
    }
}
