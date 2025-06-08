using System;

namespace Project.Areas.NhanVien.Models.DTOs
{
    public class SendSmsByWarningRequest
    {
        public Guid PatientId { get; set; }
        public Guid TreatmentRecordDetailId { get; set; }
        public DateTime FirstAbsenceDate { get; set; }
    }
}