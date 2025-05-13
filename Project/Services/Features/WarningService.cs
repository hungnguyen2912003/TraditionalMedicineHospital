using Project.Areas.Staff.Models.Entities;
using Project.Repositories.Interfaces;
using Project.Services.Features;

namespace Project.Services.Features
{
    public class WarningService
    {
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly EmailService _emailService;
        private readonly Dictionary<Guid, DateTime> _sentNotifications = new Dictionary<Guid, DateTime>();

        public WarningService
        (
            ITreatmentTrackingRepository treatmentTrackingRepository,
            EmailService emailService
        )
        {
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _emailService = emailService;
        }
        public async Task CheckAndSendConsecutiveStatusNotification(TreatmentTracking tracking)
        {
            if (tracking.Status != Models.Enums.TrackingStatus.KhongDieuTri)
                return;

            var treatmentRecordDetailId = tracking.TreatmentRecordDetailId;
            if (!treatmentRecordDetailId.HasValue)
                return;

            // Lấy tất cả tracking của cùng TreatmentRecordDetailId và Status = 3
            var allTrackings = await _treatmentTrackingRepository.GetAllAdvancedAsync();
            var relevantTrackings = allTrackings
                .Where(t => t.TreatmentRecordDetailId == treatmentRecordDetailId &&
                           t.Status == Models.Enums.TrackingStatus.KhongDieuTri &&
                           t.IsActive)
                .OrderBy(t => t.TrackingDate)
                .ToList();

            // Kiểm tra từng cặp bản ghi liên tiếp
            for (int i = 0; i < relevantTrackings.Count - 1; i++)
            {
                var firstTracking = relevantTrackings[i];
                var secondTracking = relevantTrackings[i + 1];

                // Kiểm tra xem 2 ngày có liên tiếp không
                var daysDiff = (secondTracking.TrackingDate.Date - firstTracking.TrackingDate.Date).TotalDays;
                if (daysDiff == 1)
                {
                    // Tạo key duy nhất cho cặp tracking này
                    var notificationKey = firstTracking.Id;

                    // Kiểm tra xem đã gửi thông báo cho cặp này chưa
                    if (!_sentNotifications.ContainsKey(notificationKey))
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

                            // Cập nhật thời gian gửi thông báo
                            _sentNotifications[notificationKey] = DateTime.Now;
                        }
                    }
                }
            }
        }
    }
} 