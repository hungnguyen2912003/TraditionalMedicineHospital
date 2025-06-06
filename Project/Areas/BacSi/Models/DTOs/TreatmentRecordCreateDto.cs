using Project.Areas.Staff.Models.DTOs.ReceptionDTO;

namespace Project.Areas.BacSi.Models.DTOs
{
    public class TreatmentRecordCreateDto
    {
        public TreatmentRecordDto TreatmentRecord { get; set; } = new TreatmentRecordDto();
        public AssignmentDto Assignment { get; set; } = new AssignmentDto();
        public List<TreatmentRecordDetailDto> TreatmentRecordDetails { get; set; } = new List<TreatmentRecordDetailDto>();
        public List<RegulationDto> Regulations { get; set; } = new List<RegulationDto>();
    }
}
