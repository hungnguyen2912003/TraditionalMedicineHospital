namespace Project.Areas.BacSi.Models.DTOs
{
    public class PrescriptionDto
    {
        public string Code { get; set; } = string.Empty;
        public DateTime PrescriptionDate { get; set; }
        public string? Note { get; set; }
        public decimal TotalCost { get; set; }
        //Foreign key
        public Guid TreatmentRecordId { get; set; }
        public Guid EmployeeId { get; set; }
    }
}
