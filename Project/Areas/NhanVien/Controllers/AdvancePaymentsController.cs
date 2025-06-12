using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.NhanVien.Models.ViewModels;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;
using System.Threading.Tasks;

namespace Project.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    [Authorize(Roles = "NhanVienHanhChinh")]
    [Route("tam-ung")]
    public class AdvancePaymentsController : Controller
    {
        private readonly IAdvancePaymentRepository _advancePaymentRepository;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly CodeGeneratorHelper _codeGenerator;
        public AdvancePaymentsController
        (
            IAdvancePaymentRepository advancePaymentRepository,
            ITreatmentRecordRepository treatmentRecordRepository,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            CodeGeneratorHelper codeGenerator
        )
        {
            _advancePaymentRepository = advancePaymentRepository;
            _treatmentRecordRepository = treatmentRecordRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _codeGenerator = codeGenerator;
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
            var list = await _advancePaymentRepository.GetAllAdvancedAsync();
            var viewModelList = list.Select(advancePayment => new AdvancePaymentViewModel
            {
                Id = advancePayment.Id,
                Code = advancePayment.Code,
                Note = advancePayment.Note,
                PaymentDate = advancePayment.PaymentDate,
                Type = advancePayment.Type,
                TreatmentRecordCode = advancePayment.TreatmentRecord?.Code ?? "",
                PatientName = advancePayment.TreatmentRecord?.Patient?.Name ?? "",
                Amount = advancePayment.Amount,
                Status = advancePayment.Status,
                CreatedBy = advancePayment.CreatedBy,
                UpdatedBy = advancePayment.UpdatedBy
            }).ToList();
            // Lấy danh sách các TreatmentRecord đã lập phiếu tạm ứng
            var paidTreatmentRecordIds = list.Select(p => p.TreatmentRecordId).ToList();
            // Lấy các TreatmentRecord có status 1, chưa lập phiếu tạm ứng, VÀ có ít nhất 1 Assignment.Employee.Room.DepartmentId == currentDepartmentId
            var allTreatmentRecords = (await _treatmentRecordRepository.GetAllAdvancedAsync()).ToList();
            var availableTreatmentRecords = allTreatmentRecords
                .Where(tr => (tr.Status == TreatmentStatus.DangDieuTri)
                    && !paidTreatmentRecordIds.Contains(tr.Id)
                    && tr.Assignments.Any(a => a.Employee.Room.DepartmentId == currentDepartmentId))
                .ToList();

            var allAdvancePayments = list; // list là kết quả từ _advancePaymentRepository.GetAllAdvancedAsync()
            var unpaidAdvancePayments = allAdvancePayments
                .Where(ap => ap.Status == PaymentStatus.ChuaThanhToan)
                .ToList();
            var patientIdsWithUnpaidAdvance = unpaidAdvancePayments
                .Select(ap => ap.TreatmentRecord.Patient.Id)
                .Distinct()
                .ToList();

            // Lấy danh sách bệnh nhân chỉ khi còn ít nhất 1 TreatmentRecord hợp lệ VÀ không có phiếu tạm ứng chưa thanh toán
            var patients = allTreatmentRecords
                .GroupBy(tr => tr.Patient.Id)
                .Where(g =>
                    g.Any(tr =>
                        tr.Status == TreatmentStatus.DangDieuTri
                        && !paidTreatmentRecordIds.Contains(tr.Id)
                        && tr.Assignments.Any(a => a.Employee.Room.DepartmentId == currentDepartmentId)
                    )
                    && !patientIdsWithUnpaidAdvance.Contains(g.Key)
                )
                .Select(g => new
                {
                    id = g.Key,
                    name = g.First().Patient.Name
                }).ToList();
            ViewBag.PatientsCanAdvancePayment = patients;

            // Lấy toàn bộ TreatmentRecord (Id, Code, PatientId, Status, StartDate, EndDate) phù hợp với khoa
            var treatmentRecords = allTreatmentRecords
                .Where(tr => tr.Assignments.Any(a => a.Employee.Room.DepartmentId == currentDepartmentId)
                    && (tr.Status == TreatmentStatus.DangDieuTri)
                    && !paidTreatmentRecordIds.Contains(tr.Id)
                )
                .Select(tr => new
                {
                    id = tr.Id,
                    code = tr.Code,
                    patientId = tr.Patient.Id,
                    status = tr.Status,
                    startDate = tr.StartDate,
                    endDate = tr.EndDate
                }).ToList();
            ViewBag.TreatmentRecordsCanAdvancePayment = treatmentRecords;

            // Thêm danh sách các TreatmentRecordId đã lập phiếu tạm ứng
            ViewBag.PaidTreatmentRecordIds = paidTreatmentRecordIds;

            ViewBag.AdvancePaymentCode = await _codeGenerator.GenerateUniqueCodeAsync(_advancePaymentRepository);

            return View(viewModelList);
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var advancePayment = await _advancePaymentRepository.GetByIdAdvancedAsync(id);
            if (advancePayment == null)
            {
                return NotFound();
            }

            var createdByEmployee = await _employeeRepository.GetByCodeAsync(advancePayment.CreatedBy);
            var updatedByEmployee = await _employeeRepository.GetByCodeAsync(advancePayment.UpdatedBy!);

            var viewModel = new AdvancePaymentViewModel
            {
                Id = advancePayment.Id,
                Code = advancePayment.Code,
                Note = advancePayment.Note,
                PaymentDate = advancePayment.PaymentDate,
                Type = advancePayment.Type,
                TreatmentRecordCode = advancePayment.TreatmentRecord?.Code ?? "",
                PatientName = advancePayment.TreatmentRecord?.Patient?.Name ?? "",
                Amount = advancePayment.Amount,
                Status = advancePayment.Status,
                CreatedBy = createdByEmployee?.Name ?? "Không có",
                UpdatedBy = updatedByEmployee?.Name ?? "Không có"
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

            var delList = new List<AdvancePayment>();
            foreach (var id in ids)
            {
                var entity = await _advancePaymentRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _advancePaymentRepository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var names = string.Join(", ", delList.Select(c => $"\"{c.Code}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa phiếu tạm ứng {names} thành công"
                    : $"Đã xóa các phiếu tạm ứng đã chọn thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu tạm ứng nào để xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}
