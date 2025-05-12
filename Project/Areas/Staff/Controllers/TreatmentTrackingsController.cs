using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using AutoMapper;
using Project.Areas.Staff.Models.DTOs.TrackingDTO;
using Project.Areas.Staff.Models.ViewModels;
using Project.Helpers;
using Project.Services.Features;
using Project.Areas.Staff.Models.Entities;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Yta")]
    public class TreatmentTrackingsController : Controller
    {
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        public readonly ViewBagHelper _viewBagHelper;
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly WarningService _warningService;
        private readonly CodeGeneratorHelper _codeGenerator;

        public TreatmentTrackingsController
        (
            ITreatmentTrackingRepository treatmentTrackingRepository,
            ViewBagHelper viewBagHelper,
            IMapper mapper,
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            WarningService warningService,
            CodeGeneratorHelper codeGenerator
        )
        {
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _viewBagHelper = viewBagHelper;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _warningService = warningService;
            _codeGenerator = codeGenerator;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _treatmentTrackingRepository.GetAllAdvancedAsync();
            var activeList = list.Where(x => x.IsActive).ToList();

            // Lấy mã nhân viên hiện tại từ cookie AuthToken
            string? currentEmployeeCode = null;
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
                    }
                }
            }

            // Lọc chỉ các bản ghi do bác sĩ hiện tại thực hiện
            if (!string.IsNullOrEmpty(currentEmployeeCode))
            {
                activeList = activeList.Where(x => x.CreatedBy == currentEmployeeCode).ToList();
            }

            // Lấy tất cả mã nhân viên đã chấm
            var createdByCodes = activeList
                .Where(t => !string.IsNullOrEmpty(t.CreatedBy))
                .Select(t => t.CreatedBy)
                .Distinct()
                .ToList();

            // Lấy tất cả Employee theo mã (1 lần)
            var employees = await _employeeRepository.GetByCodesAsync(createdByCodes);
            var codeNameDict = employees.ToDictionary(e => e.Code, e => e.Name);

            var dtoList = activeList.Select(t => new TreatmentTrackingDto
            {
                Id = t.Id,
                Code = t.Code,
                TrackingDate = t.TrackingDate,
                Note = t.Note,
                Status = t.Status,
                TreatmentRecordDetailId = t.TreatmentRecordDetailId ?? Guid.Empty,
                IsActive = t.IsActive,
                PatientName = t.TreatmentRecordDetail?.TreatmentRecord?.Patient?.Name,
                RoomName = t.TreatmentRecordDetail?.Room?.Name,
                EmployeeCode = t.CreatedBy
            }).ToList();

            // Then map to ViewModel
            var viewModelList = _mapper.Map<List<TreatmentTrackingViewModel>>(dtoList);
            await _viewBagHelper.BaseViewBag(ViewData);

            return View(viewModelList);
        }
    }
}