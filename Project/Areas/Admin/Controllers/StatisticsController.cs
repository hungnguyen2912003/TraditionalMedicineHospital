using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Repositories.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly ITreatmentRecordDetailRepository _treatmentRecordDetailRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPaymentRepository _paymentRepository;
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
            _treatmentRecordDetailRepository = treatmentRecordDetailRepository;
            _patientRepository = patientRepository;
            _paymentRepository = paymentRepository;
            _roomRepository = roomRepository;
            _treatmentMethodRepository = treatmentMethodRepository;
            _treatmentRecordRepository = treatmentRecordRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTreatmentStatsByDepartment
        (
            string departmentCode,
            string? startDate = null,
            string? endDate = null
        )
        {
            var detailData = await _treatmentRecordDetailRepository.GetAllAsync();
            var roomData = await _roomRepository.GetAllAdvancedAsync();
            var methodData = await _treatmentMethodRepository.GetAllAsync();

            // Nếu không truyền ngày thì lấy toàn bộ
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                DateTime start = DateTime.Parse(startDate);
                DateTime end = DateTime.Parse(endDate);
                detailData = detailData.Where(d => d.CreatedDate >= start && d.CreatedDate <= end).ToList();
            }

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
        public async Task<IActionResult> GetPatientAdmissionStats(string? startDate = null, string? endDate = null, string groupBy = "day")
        {
            var patientData = await _patientRepository.GetAllAsync();
            DateTime start, end;
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                start = DateTime.Parse(startDate);
                end = DateTime.Parse(endDate).Date.AddDays(1).AddTicks(-1);
            }
            else
            {
                // Nếu không truyền thì lấy toàn bộ
                if (!patientData.Any())
                {
                    return Json(new List<object>());
                }
                start = patientData.Min(p => p.CreatedDate);
                end = patientData.Max(p => p.CreatedDate);
            }

            var stats = new List<object>();
            if (groupBy == "day")
            {
                stats = patientData
                    .Where(p => p.CreatedDate >= start && p.CreatedDate <= end)
                    .GroupBy(p => p.CreatedDate.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),
                        patientCount = g.Count()
                    })
                    .OrderBy(x => x.date)
                    .ToList<object>();
            }
            else if (groupBy == "month")
            {
                stats = patientData
                    .Where(p => p.CreatedDate >= start && p.CreatedDate <= end)
                    .GroupBy(p => new { p.CreatedDate.Year, p.CreatedDate.Month })
                    .Select(g => new
                    {
                        date = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM"),
                        patientCount = g.Count()
                    })
                    .OrderBy(x => x.date)
                    .ToList<object>();
            }
            else if (groupBy == "year")
            {
                stats = patientData
                    .Where(p => p.CreatedDate >= start && p.CreatedDate <= end)
                    .GroupBy(p => p.CreatedDate.Year)
                    .Select(g => new
                    {
                        date = g.Key.ToString(),
                        patientCount = g.Count()
                    })
                    .OrderBy(x => x.date)
                    .ToList<object>();
            }
            return Json(stats);
        }


        [HttpGet]
        public async Task<IActionResult> GetTreatmentCompletionStats(string? startDate = null, string? endDate = null, string groupBy = "day")
        {
            var treatmentRecords = await _treatmentRecordRepository.GetAllAsync();
            // Chỉ lấy các record có trạng thái 2 hoặc 3 và có SuspendedDate
            treatmentRecords = treatmentRecords
                .Where(t => (t.Status == TreatmentStatus.DaHoanThanh || t.Status == TreatmentStatus.DaHuyBo) && t.SuspendedDate != null)
                .ToList();

            DateTime start, end;
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                start = DateTime.Parse(startDate);
                end = DateTime.Parse(endDate);
                treatmentRecords = treatmentRecords
                    .Where(t => t.SuspendedDate >= start && t.SuspendedDate <= end)
                    .ToList();
            }
            else
            {
                if (!treatmentRecords.Any())
                {
                    return Json(new List<object>());
                }
                start = treatmentRecords.Min(t => t.SuspendedDate!.Value);
                end = treatmentRecords.Max(t => t.SuspendedDate!.Value);
            }

            var stats = new List<object>();
            if (groupBy == "day")
            {
                stats = treatmentRecords
                    .GroupBy(t => t.SuspendedDate!.Value.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),
                        completedCount = g.Count(t => t.Status == TreatmentStatus.DaHoanThanh),
                        cancelledCount = g.Count(t => t.Status == TreatmentStatus.DaHuyBo)
                    })
                    .OrderBy(x => x.date)
                    .ToList<object>();
            }
            else if (groupBy == "month")
            {
                stats = treatmentRecords
                    .GroupBy(t => new { t.SuspendedDate!.Value.Year, t.SuspendedDate.Value.Month })
                    .Select(g => new
                    {
                        date = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM"),
                        completedCount = g.Count(t => t.Status == TreatmentStatus.DaHoanThanh),
                        cancelledCount = g.Count(t => t.Status == TreatmentStatus.DaHuyBo)
                    })
                    .OrderBy(x => x.date)
                    .ToList<object>();
            }
            else if (groupBy == "year")
            {
                stats = treatmentRecords
                    .GroupBy(t => t.SuspendedDate!.Value.Year)
                    .Select(g => new
                    {
                        date = g.Key.ToString(),
                        completedCount = g.Count(t => t.Status == TreatmentStatus.DaHoanThanh),
                        cancelledCount = g.Count(t => t.Status == TreatmentStatus.DaHuyBo)
                    })
                    .OrderBy(x => x.date)
                    .ToList<object>();
            }
            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetSuspendedReasonStats(string? startDate = null, string? endDate = null, string groupBy = "day")
        {
            var treatmentRecords = await _treatmentRecordRepository.GetAllAsync();
            DateTime start, end;
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                start = DateTime.Parse(startDate);
                end = DateTime.Parse(endDate);
                treatmentRecords = treatmentRecords.Where(t => t.SuspendedDate >= start && t.SuspendedDate <= end).ToList();
            }
            var result = new List<object>();
            // 3 loại lý do
            string reason1 = "Vi phạm quy định: Tự ý bỏ điều trị từ 3 ngày trong 1 đợt điều trị";
            string reason2 = "Bệnh nhân mong muốn xuất viện sớm";
            string reason3 = "Kết thúc đợt điều trị và xuất viện";
            int count1 = treatmentRecords.Count(x => x.SuspendedReason != null && x.SuspendedReason.ToLower().Contains("vi phạm quy định"));
            int count2 = treatmentRecords.Count(x => x.SuspendedReason != null && x.SuspendedReason.ToLower().Contains("bệnh nhân mong muốn xuất viện sớm"));
            int count3 = treatmentRecords.Count(x => x.SuspendedReason != null && x.SuspendedReason.ToLower().Contains("kết thúc đợt điều trị"));
            if (count1 > 0) result.Add(new { reason = reason1, count = count1 });
            if (count2 > 0) result.Add(new { reason = reason2, count = count2 });
            if (count3 > 0) result.Add(new { reason = reason3, count = count3 });
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientTypeStats(string? startDate = null, string? endDate = null, string groupBy = "day")
        {
            var allPatients = await _patientRepository.GetAllAsync();
            var allTreatmentRecords = await _treatmentRecordRepository.GetAllAsync();

            var patientIdToRecordCount = allTreatmentRecords
                .GroupBy(tr => tr.PatientId)
                .ToDictionary(g => g.Key, g => g.Count());

            int newCount = 0, oldCount = 0;
            foreach (var patient in allPatients)
            {
                int recordCount = patientIdToRecordCount.ContainsKey(patient.Id) ? patientIdToRecordCount[patient.Id] : 0;
                if (recordCount == 1)
                    newCount++;
                else if (recordCount >= 2)
                    oldCount++;
            }
            var total = newCount + oldCount;

            var stats = new List<object>();
            if (groupBy == "day")
            {
                stats.Add(new { date = DateTime.Now.ToString("yyyy-MM-dd"), newCount, oldCount });
            }
            else if (groupBy == "month")
            {
                stats.Add(new { date = DateTime.Now.ToString("yyyy-MM"), newCount, oldCount });
            }
            else if (groupBy == "year")
            {
                stats.Add(new { date = DateTime.Now.ToString("yyyy"), newCount, oldCount });
            }
            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueStats(string? startDate = null, string? endDate = null, string groupBy = "day")
        {
            var payments = (await _paymentRepository.GetAllAdvancedAsync())
                .Where(p => p.Status == PaymentStatus.DaThanhToan)
                .ToList();
            DateTime start, end;
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                start = DateTime.Parse(startDate);
                end = DateTime.Parse(endDate);
                payments = payments.Where(p => p.PaymentDate >= start && p.PaymentDate <= end).ToList();
            }
            else if (payments.Any())
            {
                start = payments.Min(p => p.PaymentDate);
                end = payments.Max(p => p.PaymentDate);
            }
            else
            {
                return Json(new List<object>());
            }

            var revenueList = new List<object>();
            if (groupBy == "day")
            {
                var grouped = payments.GroupBy(p => p.PaymentDate.Date).OrderBy(g => g.Key);
                foreach (var g in grouped)
                {
                    decimal total = 0;
                    foreach (var p in g)
                    {
                        var tr = p.TreatmentRecord;
                        var totalPrescriptionCost = tr.Prescriptions?.Sum(pre =>
                            pre.PrescriptionDetails?.Sum(d => (d.Medicine?.Price ?? 0) * d.Quantity) ?? 0) ?? 0;
                        decimal totalTreatmentMethodCost = 0;
                        foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
                        {
                            var room = detail.Room;
                            var method = room?.TreatmentMethod;
                            if (method == null) continue;
                            int count = detail.TreatmentTrackings?.Count(t => t.Status == Project.Models.Enums.TrackingStatus.CoDieuTri) ?? 0;
                            totalTreatmentMethodCost += method.Cost * count;
                        }
                        decimal totalCostBeforeInsurance = totalPrescriptionCost + totalTreatmentMethodCost;
                        decimal insuranceAmount = 0;
                        var hi = tr.Patient?.HealthInsurance;
                        if (hi != null)
                        {
                            if (hi.IsRightRoute)
                                insuranceAmount = totalCostBeforeInsurance * 0.8m;
                            else
                                insuranceAmount = totalCostBeforeInsurance * 0.6m;
                        }
                        decimal revenue = totalCostBeforeInsurance - insuranceAmount;
                        if (revenue < 0) revenue = 0;
                        total += revenue;
                    }
                    revenueList.Add(new { date = g.Key.ToString("yyyy-MM-dd"), revenue = total });
                }
            }
            else if (groupBy == "month")
            {
                var grouped = payments.GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month);
                foreach (var g in grouped)
                {
                    decimal total = 0;
                    foreach (var p in g)
                    {
                        var tr = p.TreatmentRecord;
                        var totalPrescriptionCost = tr.Prescriptions?.Sum(pre =>
                            pre.PrescriptionDetails?.Sum(d => (d.Medicine?.Price ?? 0) * d.Quantity) ?? 0) ?? 0;
                        decimal totalTreatmentMethodCost = 0;
                        foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
                        {
                            var room = detail.Room;
                            var method = room?.TreatmentMethod;
                            if (method == null) continue;
                            int count = detail.TreatmentTrackings?.Count(t => t.Status == Project.Models.Enums.TrackingStatus.CoDieuTri) ?? 0;
                            totalTreatmentMethodCost += method.Cost * count;
                        }
                        decimal totalCostBeforeInsurance = totalPrescriptionCost + totalTreatmentMethodCost;
                        decimal insuranceAmount = 0;
                        var hi = tr.Patient?.HealthInsurance;
                        if (hi != null)
                        {
                            if (hi.IsRightRoute)
                                insuranceAmount = totalCostBeforeInsurance * 0.8m;
                            else
                                insuranceAmount = totalCostBeforeInsurance * 0.6m;
                        }
                        decimal revenue = totalCostBeforeInsurance - insuranceAmount;
                        if (revenue < 0) revenue = 0;
                        total += revenue;
                    }
                    revenueList.Add(new { date = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM"), revenue = total });
                }
            }
            else if (groupBy == "year")
            {
                var grouped = payments.GroupBy(p => p.PaymentDate.Year).OrderBy(g => g.Key);
                foreach (var g in grouped)
                {
                    decimal total = 0;
                    foreach (var p in g)
                    {
                        var tr = p.TreatmentRecord;
                        var totalPrescriptionCost = tr.Prescriptions?.Sum(pre =>
                            pre.PrescriptionDetails?.Sum(d => (d.Medicine?.Price ?? 0) * d.Quantity) ?? 0) ?? 0;
                        decimal totalTreatmentMethodCost = 0;
                        foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
                        {
                            var room = detail.Room;
                            var method = room?.TreatmentMethod;
                            if (method == null) continue;
                            int count = detail.TreatmentTrackings?.Count(t => t.Status == Project.Models.Enums.TrackingStatus.CoDieuTri) ?? 0;
                            totalTreatmentMethodCost += method.Cost * count;
                        }
                        decimal totalCostBeforeInsurance = totalPrescriptionCost + totalTreatmentMethodCost;
                        decimal insuranceAmount = 0;
                        var hi = tr.Patient?.HealthInsurance;
                        if (hi != null)
                        {
                            if (hi.IsRightRoute)
                                insuranceAmount = totalCostBeforeInsurance * 0.8m;
                            else
                                insuranceAmount = totalCostBeforeInsurance * 0.6m;
                        }
                        decimal revenue = totalCostBeforeInsurance - insuranceAmount;
                        if (revenue < 0) revenue = 0;
                        total += revenue;
                    }
                    revenueList.Add(new { date = g.Key.ToString(), revenue = total });
                }
            }
            return Json(revenueList);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnpaidPaymentCountStats(string? startDate = null, string? endDate = null, string groupBy = "day")
        {
            var payments = (await _paymentRepository.GetAllAdvancedAsync())
                .Where(p => p.Status == PaymentStatus.ChuaThanhToan)
                .ToList();
            DateTime start, end;
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                start = DateTime.Parse(startDate);
                end = DateTime.Parse(endDate);
                payments = payments.Where(p => p.PaymentDate >= start && p.PaymentDate <= end).ToList();
            }
            else if (payments.Any())
            {
                start = payments.Min(p => p.PaymentDate);
                end = payments.Max(p => p.PaymentDate);
            }
            else
            {
                return Json(new List<object>());
            }
            var result = new List<object>();
            if (groupBy == "day")
            {
                var grouped = payments.GroupBy(p => p.PaymentDate.Date).OrderBy(g => g.Key);
                foreach (var g in grouped)
                {
                    result.Add(new { date = g.Key.ToString("yyyy-MM-dd"), count = g.Count() });
                }
            }
            else if (groupBy == "month")
            {
                var grouped = payments.GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month);
                foreach (var g in grouped)
                {
                    result.Add(new { date = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM"), count = g.Count() });
                }
            }
            else if (groupBy == "year")
            {
                var grouped = payments.GroupBy(p => p.PaymentDate.Year).OrderBy(g => g.Key);
                foreach (var g in grouped)
                {
                    result.Add(new { date = g.Key.ToString(), count = g.Count() });
                }
            }
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnpaidPaymentAmountStats(string? startDate = null, string? endDate = null, string groupBy = "day")
        {
            var payments = (await _paymentRepository.GetAllAdvancedAsync())
                .Where(p => p.Status == PaymentStatus.ChuaThanhToan)
                .ToList();
            DateTime start, end;
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                start = DateTime.Parse(startDate);
                end = DateTime.Parse(endDate);
                payments = payments.Where(p => p.PaymentDate >= start && p.PaymentDate <= end).ToList();
            }
            else if (payments.Any())
            {
                start = payments.Min(p => p.PaymentDate);
                end = payments.Max(p => p.PaymentDate);
            }
            else
            {
                return Json(new List<object>());
            }
            var result = new List<object>();
            if (groupBy == "day")
            {
                var grouped = payments.GroupBy(p => p.PaymentDate.Date).OrderBy(g => g.Key);
                foreach (var g in grouped)
                {
                    decimal total = 0;
                    foreach (var p in g)
                    {
                        var tr = p.TreatmentRecord;
                        var totalPrescriptionCost = tr.Prescriptions?.Sum(pre =>
                            pre.PrescriptionDetails?.Sum(d => (d.Medicine?.Price ?? 0) * d.Quantity) ?? 0) ?? 0;
                        decimal totalTreatmentMethodCost = 0;
                        foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
                        {
                            var room = detail.Room;
                            var method = room?.TreatmentMethod;
                            if (method == null) continue;
                            int count = detail.TreatmentTrackings?.Count(t => t.Status == Project.Models.Enums.TrackingStatus.CoDieuTri) ?? 0;
                            totalTreatmentMethodCost += method.Cost * count;
                        }
                        decimal totalCostBeforeInsurance = totalPrescriptionCost + totalTreatmentMethodCost;
                        decimal insuranceAmount = 0;
                        var hi = tr.Patient?.HealthInsurance;
                        if (hi != null)
                        {
                            if (hi.IsRightRoute)
                                insuranceAmount = totalCostBeforeInsurance * 0.8m;
                            else
                                insuranceAmount = totalCostBeforeInsurance * 0.6m;
                        }
                        decimal revenue = totalCostBeforeInsurance - insuranceAmount;
                        if (revenue < 0) revenue = 0;
                        total += revenue;
                    }
                    result.Add(new { date = g.Key.ToString("yyyy-MM-dd"), amount = total });
                }
            }
            else if (groupBy == "month")
            {
                var grouped = payments.GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month);
                foreach (var g in grouped)
                {
                    decimal total = 0;
                    foreach (var p in g)
                    {
                        var tr = p.TreatmentRecord;
                        var totalPrescriptionCost = tr.Prescriptions?.Sum(pre =>
                            pre.PrescriptionDetails?.Sum(d => (d.Medicine?.Price ?? 0) * d.Quantity) ?? 0) ?? 0;
                        decimal totalTreatmentMethodCost = 0;
                        foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
                        {
                            var room = detail.Room;
                            var method = room?.TreatmentMethod;
                            if (method == null) continue;
                            int count = detail.TreatmentTrackings?.Count(t => t.Status == Project.Models.Enums.TrackingStatus.CoDieuTri) ?? 0;
                            totalTreatmentMethodCost += method.Cost * count;
                        }
                        decimal totalCostBeforeInsurance = totalPrescriptionCost + totalTreatmentMethodCost;
                        decimal insuranceAmount = 0;
                        var hi = tr.Patient?.HealthInsurance;
                        if (hi != null)
                        {
                            if (hi.IsRightRoute)
                                insuranceAmount = totalCostBeforeInsurance * 0.8m;
                            else
                                insuranceAmount = totalCostBeforeInsurance * 0.6m;
                        }
                        decimal revenue = totalCostBeforeInsurance - insuranceAmount;
                        if (revenue < 0) revenue = 0;
                        total += revenue;
                    }
                    result.Add(new { date = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM"), amount = total });
                }
            }
            else if (groupBy == "year")
            {
                var grouped = payments.GroupBy(p => p.PaymentDate.Year).OrderBy(g => g.Key);
                foreach (var g in grouped)
                {
                    decimal total = 0;
                    foreach (var p in g)
                    {
                        var tr = p.TreatmentRecord;
                        var totalPrescriptionCost = tr.Prescriptions?.Sum(pre =>
                            pre.PrescriptionDetails?.Sum(d => (d.Medicine?.Price ?? 0) * d.Quantity) ?? 0) ?? 0;
                        decimal totalTreatmentMethodCost = 0;
                        foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
                        {
                            var room = detail.Room;
                            var method = room?.TreatmentMethod;
                            if (method == null) continue;
                            int count = detail.TreatmentTrackings?.Count(t => t.Status == Project.Models.Enums.TrackingStatus.CoDieuTri) ?? 0;
                            totalTreatmentMethodCost += method.Cost * count;
                        }
                        decimal totalCostBeforeInsurance = totalPrescriptionCost + totalTreatmentMethodCost;
                        decimal insuranceAmount = 0;
                        var hi = tr.Patient?.HealthInsurance;
                        if (hi != null)
                        {
                            if (hi.IsRightRoute)
                                insuranceAmount = totalCostBeforeInsurance * 0.8m;
                            else
                                insuranceAmount = totalCostBeforeInsurance * 0.6m;
                        }
                        decimal revenue = totalCostBeforeInsurance - insuranceAmount;
                        if (revenue < 0) revenue = 0;
                        total += revenue;
                    }
                    result.Add(new { date = g.Key.ToString(), amount = total });
                }
            }
            return Json(result);
        }
    }
}
