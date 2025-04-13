namespace Project.Areas.Staff.Models.DTOs
{
    public class ReceptionDto
    {
        public ReceptionPatientDto Patient { get; set; } = new ReceptionPatientDto();
        public ReceptionTreatmentRecordDto TreatmentRecord { get; set; } = new ReceptionTreatmentRecordDto();
        public ReceptionTreatmentRecordDetailDto TreatmentRecordDetail { get; set; } = new ReceptionTreatmentRecordDetailDto();
        public ReceptionAssignmentDto Assignment { get; set; } = new ReceptionAssignmentDto();
        public List<ReceptionTreatmentRecordRegulationDto> Regulations { get; set; } = new List<ReceptionTreatmentRecordRegulationDto>();
    }
}
