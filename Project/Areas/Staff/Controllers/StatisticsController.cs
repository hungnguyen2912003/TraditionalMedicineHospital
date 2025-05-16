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
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepo;
        private readonly ITreatmentRecordDetailRepository _treatmentRecordDetailRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IPrescriptionRepository _prescriptionRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly ITreatmentMethodRepository _treatmentMethodRepo;

        public StatisticsController(
            ITreatmentTrackingRepository treatmentTrackingRepo,
            ITreatmentRecordDetailRepository treatmentRecordDetailRepo,
            IPatientRepository patientRepo,
            IPaymentRepository paymentRepo,
            IPrescriptionRepository prescriptionRepo,
            IRoomRepository roomRepo,
            ITreatmentMethodRepository treatmentMethodRepo)
        {
            _treatmentTrackingRepo = treatmentTrackingRepo;
            _treatmentRecordDetailRepo = treatmentRecordDetailRepo;
            _patientRepo = patientRepo;
            _paymentRepo = paymentRepo;
            _prescriptionRepo = prescriptionRepo;
            _roomRepo = roomRepo;
            _treatmentMethodRepo = treatmentMethodRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRoomTreatmentStats(DateTime startDate, DateTime endDate)
        {
            var trackingData = await _treatmentTrackingRepo.GetAllAsync();
            var detailData = await _treatmentRecordDetailRepo.GetAllAsync();
            var roomData = await _roomRepo.GetAllAsync();

            var stats = trackingData
                .Where(t => t.Status == TrackingStatus.CoDieuTri && t.CreatedDate >= startDate && t.CreatedDate <= endDate)
                .Join(detailData,
                    tracking => tracking.TreatmentRecordDetailId,
                    detail => detail.Id,
                    (tracking, detail) => new { tracking, detail })
                .Join(roomData,
                    joined => joined.detail.RoomId,
                    room => room.Id,
                    (joined, room) => new RoomTreatmentStats
                    {
                        RoomId = room.Id,
                        RoomName = room.Name,
                        TreatmentCount = 1,
                        Date = joined.tracking.CreatedDate
                    })
                .GroupBy(x => new { x.RoomId, x.RoomName, x.Date.Date })
                .Select(g => new RoomTreatmentStats
                {
                    RoomId = g.Key.RoomId,
                    RoomName = g.Key.RoomName,
                    TreatmentCount = g.Count(),
                    Date = g.Key.Date
                })
                .ToList();

            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetTreatmentMethodStats(DateTime startDate, DateTime endDate)
        {
            var detailData = await _treatmentRecordDetailRepo.GetAllAsync();
            var roomData = await _roomRepo.GetAllAsync();
            var methodData = await _treatmentMethodRepo.GetAllAsync();

            var stats = detailData
                .Where(d => d.CreatedDate >= startDate && d.CreatedDate <= endDate)
                .Join(roomData,
                    detail => detail.RoomId,
                    room => room.Id,
                    (detail, room) => new { detail, room })
                .Join(methodData,
                    joined => joined.room.TreatmentMethodId,
                    method => method.Id,
                    (joined, method) => new TreatmentMethodStats
                    {
                        TreatmentMethodId = method.Id,
                        TreatmentMethodName = method.Name,
                        TreatmentCount = 1,
                        Date = joined.detail.CreatedDate
                    })
                .GroupBy(x => new { x.TreatmentMethodId, x.TreatmentMethodName, x.Date.Date })
                .Select(g => new TreatmentMethodStats
                {
                    TreatmentMethodId = g.Key.TreatmentMethodId,
                    TreatmentMethodName = g.Key.TreatmentMethodName,
                    TreatmentCount = g.Count(),
                    Date = g.Key.Date
                })
                .ToList();

            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientStats(DateTime startDate, DateTime endDate)
        {
            var patientData = await _patientRepo.GetAllAsync();

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

        [HttpGet]
        public async Task<IActionResult> GetFinancialStats(DateTime startDate, DateTime endDate)
        {
            var paymentData = await _paymentRepo.GetAllAsync();
            var prescriptionData = await _prescriptionRepo.GetAllAsync();

            var stats = paymentData
                .Where(p => p.CreatedDate >= startDate && p.CreatedDate <= endDate)
                .GroupBy(p => p.CreatedDate.Date)
                .Select(g => new FinancialStats
                {
                    TotalIncome = g.Count(),
                    Date = g.Key
                })
                .ToList();

            var prescriptionStats = prescriptionData
                .Where(p => p.CreatedDate >= startDate && p.CreatedDate <= endDate)
                .GroupBy(p => p.CreatedDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToList();

            foreach (var stat in stats)
            {
                var prescriptionCount = prescriptionStats.FirstOrDefault(p => p.Date == stat.Date)?.Count ?? 0;
                stat.PrescriptionCount = prescriptionCount;
            }

            return Json(stats);
        }
    }
}
