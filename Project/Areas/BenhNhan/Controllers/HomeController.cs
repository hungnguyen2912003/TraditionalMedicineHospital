using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.BenhNhan.Models.ViewModels;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Repositories.Interfaces;
using System.Globalization;

namespace Project.Areas.BenhNhan.Controllers
{
    [Area("BenhNhan")]
    [Authorize(Roles = "BenhNhan")]
    public class HomeController : Controller
    {
        private readonly IHealthInsuranceRepository _healthInsuranceRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        private readonly ITreatmentRecordDetailRepository _treatmentRecordDetailRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly JwtManager _jwtManager;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPaymentRepository _paymentRepository;

        public HomeController
        (
            IHealthInsuranceRepository healthInsuranceRepository,
            IPatientRepository patientRepository,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            ITreatmentRecordRepository treatmentRecordRepository,
            ITreatmentTrackingRepository treatmentTrackingRepository,
            ITreatmentRecordDetailRepository treatmentRecordDetailRepository,
            IPrescriptionRepository prescriptionRepository,
            IUserRepository userRepository,
            JwtManager jwtManager,
            IEmployeeRepository employeeRepository,
            IPaymentRepository paymentRepository
        )
        {
            _healthInsuranceRepository = healthInsuranceRepository;
            _patientRepository = patientRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _treatmentRecordRepository = treatmentRecordRepository;
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _treatmentRecordDetailRepository = treatmentRecordDetailRepository;
            _prescriptionRepository = prescriptionRepository;
            _userRepository = userRepository;
            _jwtManager = jwtManager;
            _employeeRepository = employeeRepository;
            _paymentRepository = paymentRepository;
        }


        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account", new { area = "Admin" });

            var (username, role) = _jwtManager.GetClaimsFromToken(token);
            if (string.IsNullOrEmpty(username) || role != "BenhNhan")
                return RedirectToAction("Login", "Account", new { area = "Admin" });

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || user.Patient == null)
                return RedirectToAction("Login", "Account", new { area = "Admin" });

            var patient = await _patientRepository.GetByIdAsync(user.Patient.Id);
            if (patient == null)
                return RedirectToAction("Login", "Account", new { area = "Admin" });

            ViewBag.PatientInfo = patient;

            var healthInsurance = await _healthInsuranceRepository.GetByPatientIdAsync(patient.Id);
            ViewBag.HealthInsurance = healthInsurance;

            // Get all treatment record details for patient's treatment records
            var treatmentRecords = await _treatmentRecordRepository.GetByPatientIdAsync(patient.Id);
            var latestRecord = treatmentRecords
                .OrderByDescending(r => r.StartDate)
                .FirstOrDefault();

            var viewModels = new List<PatientViewModel>();
            if (latestRecord == null)
            {
                ViewBag.HasPayment = false;
                ViewBag.TreatmentRecordId = null;
                ViewBag.TreatmentRecordCode = null;
                ViewBag.StartDate = null;
                ViewBag.EndDate = null;
                ViewBag.Status = null;
                ViewBag.DoctorNames = new List<string>();
                ViewBag.Prescriptions = null;
                ViewBag.DoctorList = new Dictionary<string, string>();
                return View(viewModels);
            }

            ViewBag.TreatmentRecordId = latestRecord.Id;
            var details = await _treatmentRecordDetailRepository.GetByTreatmentRecordIdAsync(latestRecord.Id);
            foreach (var detail in details)
            {
                var doctorName = "Chưa phân công";
                if (detail.TreatmentRecord?.Assignments != null)
                {
                    var assignment = detail.TreatmentRecord.Assignments.FirstOrDefault();
                    if (assignment?.Employee != null)
                        doctorName = assignment.Employee.Name;
                }

                var departmentName = detail.Room?.Department?.Name ?? "Chưa xác định";
                var treatmentMethodName = detail.Room?.TreatmentMethod?.Name ?? "Chưa phân công";
                var roomName = detail.Room?.Name ?? "Chưa xác định";

                var viewModel = new PatientViewModel
                {
                    Id = detail.Id,
                    Code = detail.Code,
                    TreatmentRecordCode = latestRecord.Code,
                    PatientName = patient.Name,
                    DoctorName = doctorName,
                    DepartmentName = departmentName,
                    TreatmentMethodName = treatmentMethodName,
                    RoomName = roomName,
                    StartDate = latestRecord.StartDate,
                    EndDate = latestRecord.EndDate,
                    Status = latestRecord.Status,
                    Note = detail.Note
                };
                viewModels.Add(viewModel);
            }
            ViewBag.TreatmentRecordCode = latestRecord.Code;
            ViewBag.StartDate = latestRecord.StartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            ViewBag.EndDate = latestRecord.EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            if (latestRecord.Status == TreatmentStatus.DangDieuTri)
                ViewBag.Status = "Đang điều trị";
            else if (latestRecord.Status == TreatmentStatus.DaHoanThanh)
                ViewBag.Status = "Đã hoàn thành";
            else if (latestRecord.Status == TreatmentStatus.DaHuyBo)
                ViewBag.Status = "Đã hủy bỏ";

            // Lấy danh sách đơn thuốc của TreatmentRecord
            var prescriptions = await _prescriptionRepository.GetByTreatmentRecordIdAsync(latestRecord.Id);
            ViewBag.Prescriptions = prescriptions;

            // Lấy danh sách mã bác sĩ từ CreatedBy
            var doctorCodes = prescriptions
                .Where(p => !string.IsNullOrEmpty(p.CreatedBy))
                .Select(p => p.CreatedBy)
                .Distinct()
                .ToList();

            // Lấy thông tin bác sĩ từ repository
            var doctorList = new Dictionary<string, string>();
            if (doctorCodes.Any())
            {
                var employees = await _employeeRepository.GetByCodesAsync(doctorCodes);
                doctorList = employees.ToDictionary(e => e.Code, e => e.Name);
            }
            ViewBag.DoctorList = doctorList;

            // Kiểm tra có payment không
            var payment = await _paymentRepository.GetByTreatmentRecordIdAsync(latestRecord.Id);
            ViewBag.HasPayment = payment != null;

            List<string> doctorNames = new List<string>();
            if (latestRecord?.Assignments != null)
            {
                doctorNames = latestRecord!.Assignments
                    .Where(a => a.Employee != null)
                    .Select(a => a.Employee.Name)
                    .Distinct()
                    .ToList();
            }

            ViewBag.DoctorNames = doctorNames;

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> GetTreatmentTracking(Guid detailId)
        {
            var detail = await _treatmentRecordDetailRepository.GetByIdAdvancedAsync(detailId);
            if (detail == null) return NotFound();

            var logs = (await _treatmentTrackingRepository.GetByDetailIdAsync(detailId))
                .OrderBy(x => x.TrackingDate)
                .ToList();

            // Lấy tất cả EmployeeId duy nhất từ logs
            var employeeIds = logs
                .Where(x => x.EmployeeId != null)
                .Select(x => x.EmployeeId!.Value)
                .Distinct()
                .ToList();

            // Lấy thông tin nhân viên
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

        [HttpGet]
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

        [HttpGet]
        public async Task<IActionResult> GetPaymentDetail(Guid treatmentRecordId)
        {
            // Lấy phiếu thanh toán mới nhất của treatmentRecordId (hoặc theo logic của bạn)
            var payment = await _paymentRepository.GetByTreatmentRecordIdAsync(treatmentRecordId);
            if (payment == null) return NotFound();

            // Tra mã người lập phiếu sang tên bác sĩ
            string createdByName = payment.CreatedBy;
            if (!string.IsNullOrEmpty(payment.CreatedBy))
            {
                var emp = await _employeeRepository.GetByCodeAsync(payment.CreatedBy);
                if (emp != null) createdByName = emp.Name;
            }

            // Map sang ViewModel hoặc trả về dữ liệu cần thiết (giống như Payments/Details)
            var tr = payment.TreatmentRecord;
            var totalPrescriptionCost = tr.Prescriptions?.Sum(pre =>
                pre.PrescriptionDetails?.Sum(d => (d.Medicine?.Price ?? 0) * d.Quantity) ?? 0) ?? 0;

            decimal totalTreatmentMethodCost = 0;
            var treatmentDetails = new List<object>();
            foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
            {
                var room = detail.Room;
                var method = room?.TreatmentMethod;
                if (method == null) continue;
                int count = detail.TreatmentTrackings?.Count(t => t.Status == TrackingStatus.CoDieuTri) ?? 0;
                var total = method.Cost * count;
                totalTreatmentMethodCost += total;
                treatmentDetails.Add(new
                {
                    departmentName = room?.Department?.Name,
                    roomName = room?.Name,
                    methodName = method.Name,
                    cost = method.Cost,
                    count = count,
                    total = total
                });
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

            var prescriptions = tr.Prescriptions?.Select(p => new
            {
                code = p.Code,
                prescriptionDate = p.PrescriptionDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                totalCost = p.PrescriptionDetails?.Sum(d => (d.Medicine?.Price ?? 0) * d.Quantity) ?? 0
            }).ToList();

            return Json(new
            {
                code = payment.Code,
                paymentDate = payment.PaymentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                createdBy = payment.CreatedBy,
                createdByName = createdByName,
                status = payment.Status,
                statusText = payment.Status == PaymentStatus.DaThanhToan ? "Đã thanh toán" : "Chưa thanh toán",
                treatmentDetails,
                totalTreatmentMethodCost,
                prescriptions,
                totalPrescriptionCost,
                insuranceAmount,
                advancePayment = tr.AdvancePayment,
                totalCostBeforeInsurance,
                finalCost,
                advanceRefund = tr.AdvancePayment - (totalCostBeforeInsurance - insuranceAmount)
            });
        }
    }
}
