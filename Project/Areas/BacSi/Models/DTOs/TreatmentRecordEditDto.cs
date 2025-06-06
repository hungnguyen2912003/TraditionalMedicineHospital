namespace Project.Areas.BacSi.Models.DTOs
{
    public class TreatmentRecordEditDto
    {
        public TreatmentRecordDto TreatmentRecord { get; set; } = new();
        public List<TreatmentRecordDetailDto> TreatmentRecordDetails { get; set; } = new();
        public List<AssignmentDto> Assignments { get; set; } = new();
        public List<RegulationDto> Regulations { get; set; } = new();
        public Guid CurrentEmployeeId { get; set; }
        public TreatmentRecordDetailDto NewTreatmentRecordDetail { get; set; } = new();
        public AssignmentDto NewAssignment { get; set; } = new();
    }
}
