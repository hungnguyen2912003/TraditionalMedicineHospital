using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.Entities;
using Project.Areas.YTa.Models.DTOs;
using Project.Areas.YTa.Models.ViewModels;
using Project.Helpers;
using Project.Models.Enums;
using Project.Repositories.Interfaces;

namespace Project.Areas.YTa.Controllers
{
    [Area("YTa")]
    [Authorize(Roles = "Yta")]
    [Route("y-ta")]
    public class TreatmentTrackingsController : Controller
    {
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        public readonly ViewBagHelper _viewBagHelper;
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        public TreatmentTrackingsController
        (
            ITreatmentTrackingRepository treatmentTrackingRepository,
            ViewBagHelper viewBagHelper,
            IMapper mapper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository
        )
        {
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _viewBagHelper = viewBagHelper;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _treatmentTrackingRepository.GetAllAdvancedAsync();

            await _viewBagHelper.BaseViewBag(ViewData);

            // Lấy mã nhân viên hiện tại từ cookie AuthToken
            string? currentEmployeeCode = null;
            string? roomName = null;
            var token = Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                var (username, role) = _viewBagHelper._jwtManager.GetClaimsFromToken(token);
                if (!string.IsNullOrEmpty(username))
                {
                    var user = await _userRepository.GetByUsernameAsync(username);
                    if (user != null && user.Employee != null)
                    {
                        currentEmployeeCode = user.Employee.Code;
                        ViewBag.CurrentEmployeeCode = currentEmployeeCode;
                        ViewBag.CurrentRole = user.Role.ToString();
                        // Lấy tên phòng của nhân viên hiện tại
                        roomName = user.Employee.Room?.Name;
                    }
                }
            }
            ViewBag.RoomName = roomName;

            // Lọc chỉ các bản ghi do bác sĩ hiện tại thực hiện
            if (!string.IsNullOrEmpty(currentEmployeeCode))
            {
                list = list.Where(x => x.CreatedBy == currentEmployeeCode).ToList();
            }

            // Lấy tất cả mã nhân viên đã chấm
            var createdByCodes = list
                .Where(t => !string.IsNullOrEmpty(t.CreatedBy))
                .Select(t => t.CreatedBy)
                .Distinct()
                .ToList();

            // Lấy tất cả Employee theo mã (1 lần)
            var employees = await _employeeRepository.GetByCodesAsync(createdByCodes);
            var codeNameDict = employees.ToDictionary(e => e.Code, e => e.Name);

            var dtoList = list.Select(t => new TreatmentTrackingDto
            {
                Id = t.Id,
                Code = t.Code,
                TrackingDate = t.TrackingDate,
                Note = t.Note,
                Status = t.Status,
                TreatmentRecordDetailId = t.TreatmentRecordDetailId ?? Guid.Empty,
                PatientName = t.TreatmentRecordDetail?.TreatmentRecord?.Patient?.Name,
                RoomName = t.TreatmentRecordDetail?.Room?.Name,
                EmployeeCode = t.CreatedBy,
                TreatmentRecordStatus = (TreatmentStatus)(t.TreatmentRecordDetail?.TreatmentRecord?.Status ?? 0)
            }).ToList();

            var viewModelList = _mapper.Map<List<TreatmentTrackingViewModel>>(dtoList)
                                    .OrderBy(vm => vm.TrackingDate)
                                    .ToList();
            return View(viewModelList);
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

            var delList = new List<TreatmentTracking>();
            foreach (var id in ids)
            {
                var entity = await _treatmentTrackingRepository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _treatmentTrackingRepository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var names = string.Join(", ", delList.Select(c => $"\"{c.Code}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa bản ghi theo dõi điều trị {names} thành công"
                    : $"Đã xóa các bản ghi theo dõi điều trị đã chọn thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy bản ghi theo dõi điều trị nào để xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}
