using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.DTOs;
using Project.Areas.Staff.Models.Entities;
using Project.Areas.Staff.Models.ViewModels;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Admin, Bacsi")]
    [Route("phieu-dieu-tri")]
    public class TreatmentRecordsController : Controller
    {
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;
        public TreatmentRecordsController
        (
            ITreatmentRecordRepository treatmentRecordRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _treatmentRecordRepository = treatmentRecordRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _treatmentRecordRepository.GetAllAdvancedAsync();
            var viewModelList = _mapper.Map<List<TreatmentRecordViewModel>>(list);
            viewModelList = viewModelList.Where(x => x.Status == TreatmentStatus.DangDieuTri).ToList();
            await _viewBagHelper.BaseViewBag(ViewData);
            var token = Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                var (username, role) = _viewBagHelper._jwtManager.GetClaimsFromToken(token);
                if (!string.IsNullOrEmpty(username))
                {
                    var user = await _userRepository.GetByUsernameAsync(username);
                    if (user != null && user.Employee != null)
                    {
                        ViewBag.CurrentEmployeeCode = user.Employee.Code;
                        ViewBag.CurrentEmployeeName = user.Employee.Name;
                        ViewBag.CurrentRole = user.Role.ToString();
                    }
                }
            }

            foreach (var vm in viewModelList)
            {
                var entity = list.FirstOrDefault(x => x.Id == vm.Id);
                if (entity != null && entity.Assignments != null)
                {
                    vm.Assignments = entity.Assignments.Select(a => new Assignment
                    {
                        Id = a.Id,
                        CreatedBy = a.CreatedBy
                    }).ToList();
                }
                else
                {
                    vm.Assignments = new List<Assignment>();
                }
            }

            return View(viewModelList);
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var treatmentRecord = await _treatmentRecordRepository.GetByIdAdvancedAsync(id);
            if (treatmentRecord == null)
            {
                return NotFound();
            }
            return View(treatmentRecord);
        }

        [HttpGet("chinh-sua/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _treatmentRecordRepository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<TreatmentRecordDto>(entity);

            ViewBag.TreatmentRecordId = entity.Id;

            await _viewBagHelper.BaseViewBag(ViewData);

            return View(dto);
        }

        [HttpPost("chinh-sua/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] TreatmentRecordDto inputDto, Guid Id)
        {
            try
            {
                var entity = await _treatmentRecordRepository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                await _treatmentRecordRepository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật thông tin đợt điều trị thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật đợt điều trị: " + ex.Message });
            }
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

            var delList = new List<TreatmentRecord>();
            var cannotDeleteList = new List<string>();

            foreach (var id in ids)
            {
                var entity = await _treatmentRecordRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    // Kiểm tra xem phiếu điều trị có đang trong thời gian điều trị không
                    if (DateTime.Now >= entity.StartDate && DateTime.Now <= entity.EndDate)
                    {
                        cannotDeleteList.Add(entity.Code);
                        continue;
                    }

                    await _treatmentRecordRepository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var codes = string.Join(", ", delList.Select(c => $"\"{c.Code}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa đợt điều trị {codes} thành công"
                    : $"Đã xóa các đợt điều trị đã chọn thành công";
                TempData["SuccessMessage"] = message;
            }

            if (cannotDeleteList.Any())
            {
                var codes = string.Join(", ", cannotDeleteList.Select(c => $"\"{c}\""));
                var message = cannotDeleteList.Count == 1
                    ? $"Không thể xóa đợt điều trị {codes} vì đang trong thời gian điều trị."
                    : $"Không thể xóa các đợt điều trị đã chọn vì đang trong thời gian điều trị.";
                TempData["ErrorMessage"] = message;
            }
            else if (!delList.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy đợt điều trị nào để xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}
