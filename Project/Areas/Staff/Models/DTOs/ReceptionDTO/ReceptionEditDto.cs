using System.ComponentModel.DataAnnotations;

namespace Project.Areas.Staff.Models.DTOs.ReceptionDTO
{
    public class ReceptionEditDto
    {
        // Patient information (editable by all doctors)
        public ReceptionPatientDto Patient { get; set; } = new ReceptionPatientDto();

        // Treatment record information (read-only)
        public ReceptionTreatmentRecordDto TreatmentRecord { get; set; } = new ReceptionTreatmentRecordDto();

        // List of treatment record details (read-only for other doctors' entries)
        public List<ReceptionTreatmentRecordDetailDto> TreatmentRecordDetails { get; set; } = new List<ReceptionTreatmentRecordDetailDto>();

        // List of assignments (read-only for other doctors' entries)
        public List<ReceptionAssignmentDto> Assignments { get; set; } = new List<ReceptionAssignmentDto>();

        // List of regulations (editable by all doctors)
        public List<ReceptionTreatmentRecordRegulationDto> Regulations { get; set; } = new List<ReceptionTreatmentRecordRegulationDto>();

        // New treatment record detail to add (editable by current doctor)
        public ReceptionTreatmentRecordDetailDto NewTreatmentRecordDetail { get; set; } = new ReceptionTreatmentRecordDetailDto();

        // New assignment to add (editable by current doctor)
        public ReceptionAssignmentDto NewAssignment { get; set; } = new ReceptionAssignmentDto();

        // Current user's employee ID
        public Guid CurrentEmployeeId { get; set; }
    }
}