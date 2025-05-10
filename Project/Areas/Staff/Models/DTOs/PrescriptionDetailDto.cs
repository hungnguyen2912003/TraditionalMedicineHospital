namespace Project.Areas.Staff.Models.DTOs
{
    public class PrescriptionDetailDto
    {
        public string Code { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        //Foreign key
        public Guid PrescriptionId { get; set; }
        public Guid MedicineId { get; set; }
    }
}
