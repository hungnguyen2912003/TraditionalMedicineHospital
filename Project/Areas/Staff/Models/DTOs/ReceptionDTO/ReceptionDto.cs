namespace Project.Areas.Staff.Models.DTOs.ReceptionDTO
{
    public class ReceptionDto
    {
        public ReceptionPatientDto Patient { get; set; } = new ReceptionPatientDto();
        public ReceptionTreatmentRecordDto TreatmentRecord { get; set; } = new ReceptionTreatmentRecordDto();
        public ReceptionAssignmentDto Assignment { get; set; } = new ReceptionAssignmentDto();
        public ReceptionTreatmentRecordDetailDto TreatmentRecordDetail { get; set; } = new ReceptionTreatmentRecordDetailDto();
        public List<ReceptionTreatmentRecordDetailDto> TreatmentRecordDetails { get; set; } = new List<ReceptionTreatmentRecordDetailDto>();
        public List<ReceptionTreatmentRecordRegulationDto> Regulations { get; set; } = new List<ReceptionTreatmentRecordRegulationDto>();
    }
}
