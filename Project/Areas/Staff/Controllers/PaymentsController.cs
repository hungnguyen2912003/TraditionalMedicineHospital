using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using Project.Areas.Staff.Models.ViewModels;
using Project.Helpers;
using Project.Areas.Staff.Models.Entities;
using Project.Models.Enums;
using Repositories.Interfaces;
using System.Globalization;
using Project.Extensions;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Bacsi")]
    [Route("phieu-thanh-toan")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly ITreatmentRecordDetailRepository _treatmentRecordDetailRepository;
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;

        public PaymentsController
        (
            IPaymentRepository paymentRepository,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator,
            ITreatmentRecordRepository treatmentRecordRepository,
            ITreatmentRecordDetailRepository treatmentRecordDetailRepository,
            ITreatmentTrackingRepository treatmentTrackingRepository,
            IEmployeeRepository employeeRepository,
            IPrescriptionRepository prescriptionRepository
        )
        {
            _paymentRepository = paymentRepository;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
            _treatmentRecordRepository = treatmentRecordRepository;
            _treatmentRecordDetailRepository = treatmentRecordDetailRepository;
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _employeeRepository = employeeRepository;
            _prescriptionRepository = prescriptionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentRepository.GetAllAdvancedAsync();
            var viewModels = payments.Select(p =>
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
                    int count = detail.TreatmentTrackings?.Count(t => t.Status == TrackingStatus.CoDieuTri) ?? 0;
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

                decimal finalCost = totalCostBeforeInsurance - insuranceAmount - tr.AdvancePayment;
                if (finalCost < 0) finalCost = 0;

                return new PaymentViewModel
                {
                    Id = p.Id,
                    Code = p.Code,
                    PaymentDate = p.PaymentDate,
                    TreatmentRecordCode = tr.Code,
                    PatientName = tr.Patient!.Name,
                    TotalPrescriptionCost = totalPrescriptionCost,
                    TotalTreatmentMethodCost = totalTreatmentMethodCost,
                    InsuranceAmount = insuranceAmount,
                    TotalCost = totalCostBeforeInsurance,
                    FinalCost = finalCost,
                    Status = p.Status == PaymentStatus.DaThanhToan ? PaymentStatus.DaThanhToan : PaymentStatus.ChuaThanhToan
                };
            });

            // Lấy danh sách bệnh nhân
            var patients = (await _treatmentRecordRepository.GetAllAdvancedAsync())
                .Select(tr => tr.Patient)
                .Distinct()
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name
                }).ToList();
            ViewBag.Patients = patients;

            // Lấy toàn bộ TreatmentRecord (Id, Code, PatientId, Status, StartDate, EndDate)
            var treatmentRecords = (await _treatmentRecordRepository.GetAllAdvancedAsync())
                .Select(tr => new
                {
                    id = tr.Id,
                    code = tr.Code,
                    patientId = tr.Patient.Id,
                    status = tr.Status,
                    startDate = tr.StartDate,
                    endDate = tr.EndDate,
                    patientHealthInsuranceNumber = tr.Patient.HealthInsurance != null ? tr.Patient.HealthInsurance.Number : null,
                    patientHealthInsuranceExpiredDate = tr.Patient.HealthInsurance != null ? tr.Patient.HealthInsurance.ExpiryDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                    patientHealthInsurancePlaceOfRegistration = tr.Patient.HealthInsurance != null ? EnumExtensions.GetDisplayName(tr.Patient.HealthInsurance.PlaceOfRegistration) : null,
                    patientHealthInsuranceIsRightRoute = tr.Patient.HealthInsurance != null ? tr.Patient.HealthInsurance.IsRightRoute : (bool?)null
                }).ToList();
            ViewBag.TreatmentRecordsCanPayment = treatmentRecords;

            // Thêm danh sách các TreatmentRecordId đã lập phiếu thanh toán
            var paidTreatmentRecordIds = payments.Select(p => p.TreatmentRecordId).ToList();
            ViewBag.PaidTreatmentRecordIds = paidTreatmentRecordIds;

            ViewBag.PaymentCode = await _codeGenerator.GenerateUniqueCodeAsync(_paymentRepository);

            return View(viewModels);
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var payment = await _paymentRepository.GetByIdAdvancedAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            var tr = payment.TreatmentRecord;
            var totalPrescriptionCost = tr.Prescriptions?.Sum(pre =>
                pre.PrescriptionDetails?.Sum(d => (d.Medicine?.Price ?? 0) * d.Quantity) ?? 0) ?? 0;

            decimal totalTreatmentMethodCost = 0;
            foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
            {
                var room = detail.Room;
                var method = room?.TreatmentMethod;
                if (method == null) continue;
                int count = detail.TreatmentTrackings?.Count(t => t.Status == TrackingStatus.CoDieuTri) ?? 0;
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

            decimal finalCost = totalCostBeforeInsurance - insuranceAmount - tr.AdvancePayment;
            if (finalCost < 0) finalCost = 0;

            var viewModel = new PaymentViewModel
            {
                Id = payment.Id,
                Code = payment.Code,
                Note = payment.Note,
                PaymentDate = payment.PaymentDate,
                CreatedDate = payment.CreatedDate,
                CreatedBy = payment.CreatedBy,
                UpdatedDate = payment.UpdatedDate,
                UpdatedBy = payment.UpdatedBy,
                TreatmentRecordCode = tr.Code,
                PatientName = tr.Patient!.Name,
                TotalPrescriptionCost = totalPrescriptionCost,
                TotalTreatmentMethodCost = totalTreatmentMethodCost,
                InsuranceAmount = insuranceAmount,
                AdvancePayment = tr.AdvancePayment,
                TotalCost = totalCostBeforeInsurance,
                FinalCost = finalCost,
                Status = payment.Status,
                Prescriptions = tr.Prescriptions?.ToList() ?? new(),
                PatientHealthInsuranceNumber = hi?.Number,
                PatientHealthInsuranceExpiredDate = hi?.ExpiryDate,
                PatientHealthInsurancePlaceOfRegistration = hi?.PlaceOfRegistration,
                PatientHealthInsuranceIsRightRoute = hi?.IsRightRoute,
                TreatmentDetails = tr.TreatmentRecordDetails?.ToList() ?? new()
            };

            return View(viewModel);
        }

        [HttpPost("xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] string selectedIds)
        {
            var ids = new List<Guid>();
            foreach (var id in selectedIds.Split(','))
            {
                if (Guid.TryParse(id, out var parsedId))
                {
                    ids.Add(parsedId);
                }
            }

            var delList = new List<Payment>();
            foreach (var id in ids)
            {
                var entity = await _paymentRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _paymentRepository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var names = string.Join(", ", delList.Select(c => $"\"{c.Code}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa phiếu thanh toán {names} thành công"
                    : $"Đã xóa các phiếu thanh toán đã chọn thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu thanh toán nào để xóa.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet("get-treatment-tracking")]
        public async Task<IActionResult> GetTreatmentTracking(Guid detailId)
        {
            var detail = await _treatmentRecordDetailRepository.GetByIdAdvancedAsync(detailId);
            if (detail == null) return NotFound();

            var logs = (await _treatmentTrackingRepository.GetByDetailIdAsync(detailId))
                .OrderBy(x => x.TrackingDate)
                .ToList();

            var employeeIds = logs
                .Where(x => x.EmployeeId != null)
                .Select(x => x.EmployeeId!.Value)
                .Distinct()
                .ToList();

            var employees = await _employeeRepository.GetByIdsAsync(employeeIds);
            var employeeDict = employees.ToDictionary(e => e.Id, e => e.Name);

            var result = new
            {
                methodName = detail.Room?.TreatmentMethod?.Name ?? "",
                roomName = detail.Room?.Name ?? "",
                logs = logs.Select(x => new
                {
                    date = x.TrackingDate.ToString("dd/MM/yyyy"),
                    status = x.Status,
                    staff = x.EmployeeId != null && employeeDict.ContainsKey(x.EmployeeId.Value)
                        ? employeeDict[x.EmployeeId.Value]
                        : "Không xác định",
                    note = x.Note
                })
            };
            return Json(result);
        }

        [HttpGet("get-prescription-detail")]
        public async Task<IActionResult> GetPrescriptionDetail(Guid prescriptionId)
        {
            var prescription = await _prescriptionRepository.GetByIdAdvancedAsync(prescriptionId);
            if (prescription == null) return NotFound();

            var details = prescription.PrescriptionDetails.Select(d => new
            {
                medicineName = d.Medicine?.Name ?? "",
                quantity = d.Quantity,
                price = d.Medicine != null ? d.Medicine.Price : 0,
                total = (d.Medicine != null ? d.Medicine.Price : 0) * d.Quantity
            }).ToList();

            return Json(details);
        }
    }
}
