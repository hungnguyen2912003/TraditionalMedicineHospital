namespace Project.Areas.BacSi.Models.DTOs
{
    public class PrescriptionCreateRequest
    {
        public string Code { get; set; } = string.Empty;
        public DateTime PrescriptionDate { get; set; }
        public string? Note { get; set; }
        public Guid TreatmentRecordId { get; set; }
        public Guid EmployeeId { get; set; }
        public List<PrescriptionDetailCreateDto> Details { get; set; } = new List<PrescriptionDetailCreateDto>();
    }

    public class PrescriptionDetailCreateDto
    {
        public Guid MedicineId { get; set; }
        public Guid PrescriptionId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
