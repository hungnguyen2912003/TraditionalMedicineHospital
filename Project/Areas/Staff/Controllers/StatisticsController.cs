using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.Statistics;
using Project.Repositories.Interfaces;
using Repositories.Interfaces;
using Project.Models.Enums;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Admin, Bacsi")]
    public class StatisticsController : Controller
    {
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly ITreatmentRecordDetailRepository _treatmentRecordDetailRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly ITreatmentMethodRepository _treatmentMethodRepository;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;

        public StatisticsController(
            ITreatmentTrackingRepository treatmentTrackingRepository,
            ITreatmentRecordDetailRepository treatmentRecordDetailRepository,
            IPatientRepository patientRepository,
            IPaymentRepository paymentRepository,
            IPrescriptionRepository prescriptionRepository,
            IRoomRepository roomRepository,
            ITreatmentMethodRepository treatmentMethodRepository,
            ITreatmentRecordRepository treatmentRecordRepository)
        {
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _treatmentRecordDetailRepository = treatmentRecordDetailRepository;
            _patientRepository = patientRepository;
            _paymentRepository = paymentRepository;
            _prescriptionRepository = prescriptionRepository;
            _roomRepository = roomRepository;
            _treatmentMethodRepository = treatmentMethodRepository;
            _treatmentRecordRepository = treatmentRecordRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientStats(DateTime startDate, DateTime endDate)
        {
            var patientData = await _patientRepository.GetAllAsync();

            var stats = patientData
                .Where(p => p.CreatedDate >= startDate && p.CreatedDate <= endDate)
                .GroupBy(p => p.CreatedDate.Date)
                .Select(g => new PatientStats
                {
                    TotalPatients = g.Count(),
                    WarningPatients = 0,
                    Date = g.Key
                })
                .ToList();

            return Json(stats);
        }

        // [HttpGet]
        // public async Task<IActionResult> GetFinancialStats(DateTime startDate, DateTime endDate)
        // {
        //     var paymentData = await _paymentRepository.GetAllAsync();
        //     var detailData = await _treatmentRecordDetailRepository.GetAllAsync();

        //     var stats = paymentData
        //         .Where(p => p.CreatedDate >= startDate && p.CreatedDate <= endDate)
        //         .Join(detailData,
        //             payment => payment.TreatmentRecordDetailId,
        //             detail => detail.Id,
        //             (payment, detail) => new { 
        //                 Date = payment.CreatedDate.Date,
        //                 Amount = detail.Price * detail.Quantity
        //             })
        //         .GroupBy(x => x.Date)
        //         .Select(g => new FinancialStats
        //         {
        //             TotalIncome = g.Sum(x => x.Amount),
        //             Date = g.Key
        //         })
        //         .OrderBy(x => x.Date)
        //         .ToList();

        //     return Json(stats);
        // }

        [HttpGet]
        public async Task<IActionResult> GetTreatmentStatsByDepartment(string departmentCode)
        {
            var detailData = await _treatmentRecordDetailRepository.GetAllAsync();
            var roomData = await _roomRepository.GetAllAdvancedAsync();
            var methodData = await _treatmentMethodRepository.GetAllAsync();

            var stats = detailData
                .Join(roomData,
                    detail => detail.RoomId,
                    room => room.Id,
                    (detail, room) => new { detail, room })
                .Where(x => x.room.Department.Code == departmentCode)
                .Join(methodData,
                    joined => joined.room.TreatmentMethodId,
                    method => method.Id,
                    (joined, method) => new
                    {
                        TreatmentMethodId = method.Id,
                        TreatmentMethodName = method.Name,
                        Count = 1
                    })
                .GroupBy(x => new { x.TreatmentMethodId, x.TreatmentMethodName })
                .Select(g => new
                {
                    MethodName = g.Key.TreatmentMethodName,
                    TotalTreatments = g.Count()
                })
                .ToList();

            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientStatsByRoom(string departmentCode)
        {
            var detailData = await _treatmentRecordDetailRepository.GetAllAsync();
            var roomData = await _roomRepository.GetAllAdvancedAsync();

            var stats = detailData
                .Join(roomData,
                    detail => detail.RoomId,
                    room => room.Id,
                    (detail, room) => new { detail, room })
                .Where(x => x.room.Department.Code == departmentCode)
                .GroupBy(x => new { x.room.Id, x.room.Name })
                .Select(g => new
                {
                    RoomName = g.Key.Name,
                    PatientCount = g.Count()
                })
                .ToList();

            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientAdmissionStats(string period = "month")
        {
            var patientData = await _patientRepository.GetAllAsync();
            var currentDate = DateTime.Now;
            var startDate = period.ToLower() == "month"
                ? currentDate.AddMonths(-1)
                : currentDate.AddYears(-1);

            var stats = patientData
                .Where(p => p.CreatedDate >= startDate && p.CreatedDate <= currentDate)
                .GroupBy(p => period.ToLower() == "month"
                    ? p.CreatedDate.Date
                    : new DateTime(p.CreatedDate.Year, p.CreatedDate.Month, 1))
                .Select(g => new
                {
                    Date = g.Key,
                    PatientCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetTreatmentCompletionStats(int year)
        {
            var treatmentRecords = await _treatmentRecordRepository.GetAllAsync();

            var stats = Enumerable.Range(1, 12)
                .Select(month => new
                {
                    Month = month,
                    CompletedCount = treatmentRecords.Count(t =>
                        t.Status == TreatmentStatus.DaHoanThanh &&
                        t.CreatedDate.Year == year &&
                        t.CreatedDate.Month == month),
                    CancelledCount = treatmentRecords.Count(t =>
                        t.Status == TreatmentStatus.DaHuyBo &&
                        t.CreatedDate.Year == year &&
                        t.CreatedDate.Month == month)
                })
                .ToList();

            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetFollowUpTreatmentStats(int year)
        {
            var treatmentRecords = await _treatmentRecordRepository.GetAllAsync();

            var stats = treatmentRecords
                .GroupBy(t => t.PatientId)
                .Where(g => g.Count() > 1) // Lấy các bệnh nhân có nhiều hơn 1 đợt điều trị
                .SelectMany(g => g
                    .Where(t => t.Status == TreatmentStatus.DaHoanThanh) // Chỉ tính các đợt đã hoàn thành
                    .OrderBy(t => t.CreatedDate)
                    .Skip(1) // Bỏ qua đợt điều trị đầu tiên
                    .Where(t => t.CreatedDate.Year == year)) // Lọc theo năm
                    .GroupBy(t => t.CreatedDate.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        FollowUpCount = g.Count()
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

            // Đảm bảo có đủ 12 tháng với số lượng 0 nếu không có dữ liệu
            var fullStats = Enumerable.Range(1, 12)
                .GroupJoin(
                    stats,
                    month => month,
                    stat => stat.Month,
                    (month, statGroup) => new
                    {
                        Month = month,
                        FollowUpCount = statGroup.FirstOrDefault()?.FollowUpCount ?? 0
                    }
                )
                .ToList();

            return Json(fullStats);
        }
    }
}