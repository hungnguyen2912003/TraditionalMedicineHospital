using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.NhanVien.Models.ViewModels;
using Project.Extensions;
using Project.Helpers;
using Project.Library;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Repositories.Interfaces;
using System.Globalization;

namespace Project.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    [Authorize(Roles = "NhanVienHanhChinh")]
    [Route("/thanh-toan")]
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
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly JwtManager _jwtManager;

        public PaymentsController
        (
            IPaymentRepository paymentRepository,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator,
            ITreatmentRecordRepository treatmentRecordRepository,
            ITreatmentRecordDetailRepository treatmentRecordDetailRepository,
            ITreatmentTrackingRepository treatmentTrackingRepository,
            IEmployeeRepository employeeRepository,
            IPrescriptionRepository prescriptionRepository,
            IUserRepository userRepository,
            IConfiguration configuration,
            JwtManager jwtManager
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
            _userRepository = userRepository;
            _configuration = configuration;
            _jwtManager = jwtManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lấy thông tin nhân viên hiện tại từ token
            var token = Request.Cookies["AuthToken"];
            Guid? currentDepartmentId = null;
            if (!string.IsNullOrEmpty(token))
            {
                var (username, role) = _viewBagHelper._jwtManager.GetClaimsFromToken(token);
                if (!string.IsNullOrEmpty(username))
                {
                    var user = await _userRepository.GetByUsernameAsync(username);
                    if (user != null && user.Employee != null)
                    {
                        currentDepartmentId = user.Employee.Room.DepartmentId;
                    }
                }
            }

            var payments = await _paymentRepository.GetAllForListViewAsync();
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
                if (hi != null && hi.ExpiryDate > DateTime.UtcNow)
                {
                    if (hi.IsRightRoute)
                        insuranceAmount = totalCostBeforeInsurance * 0.8m;
                    else
                        insuranceAmount = totalCostBeforeInsurance * 0.6m;
                }

                decimal finalCost = totalCostBeforeInsurance - insuranceAmount - (tr.AdvancePayment ?? 0);
                if (finalCost < 0) finalCost = 0;

                return new PaymentViewModel
                {
                    Id = p.Id,
                    Code = p.Code,
                    PaymentDate = p.PaymentDate,
                    Type = p.Type,
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

            // Lấy danh sách các TreatmentRecord đã lập phiếu thanh toán
            var paidTreatmentRecordIds = payments.Select(p => p.TreatmentRecordId).ToList();

            // Lấy các TreatmentRecord có status 2 hoặc 3, chưa lập phiếu thanh toán, VÀ có ít nhất 1 Assignment.Employee.Room.DepartmentId == currentDepartmentId
            var allTreatmentRecords = (await _treatmentRecordRepository.GetAllAdvancedAsync()).ToList();
            var availableTreatmentRecords = allTreatmentRecords
                .Where(tr => (tr.Status == TreatmentStatus.DaHoanThanh || tr.Status == TreatmentStatus.DaHuyBo)
                    && !paidTreatmentRecordIds.Contains(tr.Id)
                    && tr.Assignments.Any(a => a.Employee.Room.DepartmentId == currentDepartmentId))
                .ToList();

            // Lấy danh sách bệnh nhân chỉ khi còn ít nhất 1 TreatmentRecord hợp lệ
            var patients = allTreatmentRecords
                .GroupBy(tr => tr.Patient.Id)
                .Where(g => g.Any(tr =>
                    (tr.Status == TreatmentStatus.DaHoanThanh || tr.Status == TreatmentStatus.DaHuyBo)
                    && !paidTreatmentRecordIds.Contains(tr.Id)
                    && tr.Assignments.Any(a => a.Employee.Room.DepartmentId == currentDepartmentId)
                ))
                .Select(g => new
                {
                    id = g.Key,
                    name = g.First().Patient.Name
                }).ToList();

            ViewBag.PatientsCanPayment = patients;

            // Lấy toàn bộ TreatmentRecord (Id, Code, PatientId, Status, StartDate, EndDate) phù hợp với khoa
            var treatmentRecords = allTreatmentRecords
                .Where(tr => tr.Assignments.Any(a => a.Employee.Room.DepartmentId == currentDepartmentId)
                    && (tr.Status == TreatmentStatus.DaHoanThanh || tr.Status == TreatmentStatus.DaHuyBo)
                    && !paidTreatmentRecordIds.Contains(tr.Id)
                )
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
                    patientHealthInsurancePlaceOfRegistration = tr.Patient.HealthInsurance != null ? tr.Patient.HealthInsurance.PlaceOfRegistration.GetDisplayName() : null,
                    patientHealthInsuranceIsRightRoute = tr.Patient.HealthInsurance != null ? tr.Patient.HealthInsurance.IsRightRoute : (bool?)null
                }).ToList();
            ViewBag.TreatmentRecordsCanPayment = treatmentRecords;

            // Thêm danh sách các TreatmentRecordId đã lập phiếu thanh toán
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
            if (hi != null && hi.ExpiryDate > DateTime.UtcNow)
            {
                if (hi.IsRightRoute)
                    insuranceAmount = totalCostBeforeInsurance * 0.8m;
                else
                    insuranceAmount = totalCostBeforeInsurance * 0.6m;
            }

            decimal finalCost = totalCostBeforeInsurance - insuranceAmount - (tr.AdvancePayment ?? 0);
            if (finalCost < 0) finalCost = 0;

            var createdByEmployee = await _employeeRepository.GetByCodeAsync(payment.CreatedBy);
            var updatedByEmployee = await _employeeRepository.GetByCodeAsync(payment.UpdatedBy!);

            var viewModel = new PaymentViewModel
            {
                Id = payment.Id,
                Code = payment.Code,
                Note = payment.Note,
                PaymentDate = payment.PaymentDate,
                Type = payment.Type,
                CreatedDate = payment.CreatedDate,
                CreatedBy = payment.CreatedBy,
                CreatedByName = createdByEmployee?.Name ?? "Không có",
                UpdatedDate = payment.UpdatedDate,
                UpdatedBy = payment.UpdatedBy,
                UpdatedByName = updatedByEmployee?.Name ?? "Không có",
                TreatmentRecordCode = tr.Code,
                PatientName = tr.Patient!.Name,
                TotalPrescriptionCost = totalPrescriptionCost,
                TotalTreatmentMethodCost = totalTreatmentMethodCost,
                InsuranceAmount = insuranceAmount,
                AdvancePayment = (tr.AdvancePayment ?? 0),
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
