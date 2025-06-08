using System;

namespace Project.Areas.NhanVien.Models.DTOs
{
    public class SendMailByWarningRequest
    {
        public Guid PatientId { get; set; }
        public Guid TreatmentRecordDetailId { get; set; }
    }
}