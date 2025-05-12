using Project.Areas.Staff.Models.Entities;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Services.Features
{
    public class WarningService
    {
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly EmailService _emailService;

        public WarningService
        (
            ITreatmentTrackingRepository treatmentTrackingRepository,
            EmailService emailService
        )
        {
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _emailService = emailService;
        }

        public async Task CheckAndSendAbsenceNotification(TreatmentTracking tracking)
        {
            // Lấy danh sách tracking của bệnh nhân này trong 2 ngày gần nhất
            var patientTrackings = await _treatmentTrackingRepository.GetAllAdvancedAsync();
            var patientRecentTrackings = patientTrackings
                .Where(t => t.TreatmentRecordDetail?.TreatmentRecord?.PatientId == 
                           tracking.TreatmentRecordDetail?.TreatmentRecord?.PatientId &&
                           t.IsActive &&
                           t.Status == Models.Enums.TrackingStatus.KhongDieuTri)
                .OrderByDescending(t => t.TrackingDate)
                .Take(2)
                .ToList();

            // Nếu có đủ 2 bản ghi vắng mặt liên tiếp
            if (patientRecentTrackings.Count == 2)
            {
                var firstTracking = patientRecentTrackings[1];
                var secondTracking = patientRecentTrackings[0];

                // Kiểm tra xem 2 ngày có liên tiếp không
                var daysDiff = (secondTracking.TrackingDate.Date - firstTracking.TrackingDate.Date).TotalDays;
                if (daysDiff == 1)
                {
                    var patient = tracking.TreatmentRecordDetail?.TreatmentRecord?.Patient;
                    if (patient != null && !string.IsNullOrEmpty(patient.EmailAddress))
                    {
                        var subject = "Nhắc nhở điều trị - Bệnh viện Y học cổ truyền Nha Trang";
                        var body = $@"
                            <h2>Xin chào {patient.Name},</h2>
                            <p>Hệ thống ghi nhận bạn đã vắng mặt trong 2 ngày liên tiếp:</p>
                            <ul>
                                <li>Ngày {firstTracking.TrackingDate.ToString("dd/MM/yyyy")}</li>
                                <li>Ngày {secondTracking.TrackingDate.ToString("dd/MM/yyyy")}</li>
                            </ul>
                            <p>Để đảm bảo hiệu quả điều trị, vui lòng đến bệnh viện để tiếp tục quá trình điều trị.</p>
                            <p>Nếu bạn có lý do đặc biệt, vui lòng liên hệ với bác sĩ điều trị của bạn.</p>
                            <p>Trân trọng,<br>Hệ thống quản lý Bệnh viện Y học cổ truyền Nha Trang</p>";

                        await _emailService.SendEmailAsync(patient.EmailAddress, subject, body);
                    }
                }
            }
        }
    }
} 