using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.BacSi.Models.ViewModels;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Repositories.Interfaces;
using Project.Models.Enums;
using Project.Services.Features;

namespace Project.Areas.BacSi.Controllers
{
    [Area("BacSi")]
    [Authorize(Roles = "BacSi")]
    [Route("don-thuoc")]
    public class PrescriptionsController : Controller
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IPrescriptionDetailRepository _prescriptionDetailRepository;
        private readonly IMedicineRepository _medicineRepository;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly JwtManager _jwtManager;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;

        public PrescriptionsController(
            IPrescriptionRepository prescriptionRepository,
            IPrescriptionDetailRepository prescriptionDetailRepository,
            IMedicineRepository medicineRepository,
            ITreatmentRecordRepository treatmentRecordRepository,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            JwtManager jwtManager,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _prescriptionRepository = prescriptionRepository;
            _prescriptionDetailRepository = prescriptionDetailRepository;
            _medicineRepository = medicineRepository;
            _treatmentRecordRepository = treatmentRecordRepository;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _jwtManager = jwtManager;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get user info from token
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "Người dùng chưa đăng nhập" });
            }

            var (username, role) = _jwtManager.GetClaimsFromToken(token);
            if (string.IsNullOrEmpty(username))
            {
                Response.Cookies.Delete("AuthToken");
                return Json(new { success = false, message = "Token không hợp lệ." });
            }

            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || user.Employee == null)
            {
                return Json(new { success = false, message = "Người dùng không hợp lệ" });
            }

            ViewBag.CurrentEmployeeCode = user.Employee.Code;

            var list = await _prescriptionRepository.GetAllAdvancedAsync();
            var viewModelList = list
                .Where(p => p.TreatmentRecord?.Status == TreatmentStatus.DangDieuTri)
                .Select(p => new PrescriptionViewModel
                {
                    Id = p.Id,
                    Code = p.Code,
                    PrescriptionDate = p.PrescriptionDate,
                    Note = p.Note,
                    TreatmentRecordId = p.TreatmentRecordId,
                    TreatmentRecordCode = p.TreatmentRecord?.Code ?? string.Empty,
                    PatientName = p.TreatmentRecord?.Patient?.Name ?? string.Empty,
                    EmployeeId = p.EmployeeId,
                    EmployeeName = p.Employee?.Name ?? string.Empty,
                    PrescriptionDetails = p.PrescriptionDetails?.Select(d => new PrescriptionDetailViewModel
                    {
                        MedicineId = d.MedicineId,
                        MedicineName = d.Medicine?.Name ?? string.Empty,
                        Quantity = d.Quantity,
                        Price = d.Medicine?.Price
                    }).ToList() ?? new List<PrescriptionDetailViewModel>(),
                    Assignments = p.TreatmentRecord?.Assignments?.Select(a => new AssignmentViewModel
                    {
                        CreatedBy = a.CreatedBy,
                        EmployeeName = a.Employee?.Name ?? string.Empty
                    }).ToList() ?? new List<AssignmentViewModel>()
                }).OrderBy(x => x.PrescriptionDate).ToList();
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(viewModelList);
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var prescription = await _prescriptionRepository.GetByIdAdvancedAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }

            var createdByEmployee = await _employeeRepository.GetByCodeAsync(prescription.CreatedBy);
            var updatedByEmployee = await _employeeRepository.GetByCodeAsync(prescription.UpdatedBy!);
            var viewModel = new PrescriptionViewModel
            {
                Id = prescription.Id,
                Code = prescription.Code,
                PrescriptionDate = prescription.PrescriptionDate,
                Note = prescription.Note,
                TreatmentRecordId = prescription.TreatmentRecordId,
                TreatmentRecordCode = prescription.TreatmentRecord?.Code ?? string.Empty,
                PatientName = prescription.TreatmentRecord?.Patient?.Name ?? string.Empty,
                EmployeeId = prescription.EmployeeId,
                EmployeeName = prescription.Employee?.Name ?? string.Empty,
                CreatedDate = prescription.CreatedDate,
                CreatedBy = prescription.CreatedBy,
                CreatedByName = createdByEmployee?.Name ?? string.Empty,
                UpdatedDate = prescription.UpdatedDate,
                UpdatedBy = prescription.UpdatedBy,
                UpdatedByName = updatedByEmployee?.Name ?? string.Empty,
                PrescriptionDetails = prescription.PrescriptionDetails?.Select(d => new PrescriptionDetailViewModel
                {
                    MedicineId = d.MedicineId,
                    MedicineName = d.Medicine?.Name ?? string.Empty,
                    Quantity = d.Quantity,
                    Price = d.Medicine?.Price
                }).ToList() ?? new List<PrescriptionDetailViewModel>()
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

            var delList = new List<Prescription>();
            foreach (var id in ids)
            {
                var entity = await _prescriptionRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _prescriptionRepository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var names = string.Join(", ", delList.Select(c => $"\"{c.Code}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa đơn thuốc {names} thành công"
                    : $"Đã xóa các đơn thuốc đã chọn thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn thuốc nào để xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}
