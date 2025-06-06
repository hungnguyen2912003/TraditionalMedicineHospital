using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Project.Areas.Staff.Models.DTOs.ReceptionDTO
{
    public class ReceptionEditDto
    {
        // Patient information (editable by all doctors)
        public ReceptionPatientDto Patient { get; set; } = new();

        // Treatment record information (read-only)
        public ReceptionTreatmentRecordDto TreatmentRecord { get; set; } = new();

        // List of treatment record details (read-only for other doctors' entries)
        public List<ReceptionTreatmentRecordDetailDto> TreatmentRecordDetails { get; set; } = new();

        // List of assignments (read-only for other doctors' entries)
        public List<ReceptionAssignmentDto> Assignments { get; set; } = new();

        // List of regulations (editable by all doctors)
        public List<ReceptionTreatmentRecordRegulationDto> Regulations { get; set; } = new();

        // New treatment record detail to add (editable by current doctor)
        public ReceptionTreatmentRecordDetailDto NewTreatmentRecordDetail { get; set; } = new();

        // New assignment to add (editable by current doctor)
        public ReceptionAssignmentDto NewAssignment { get; set; } = new();

        // New regulation to add (editable by current doctor)
        public ReceptionTreatmentRecordRegulationDto NewRegulation { get; set; } = new();

        // Current user's employee ID
        public Guid CurrentEmployeeId { get; set; }
    }
}