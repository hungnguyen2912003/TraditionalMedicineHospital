using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.Entities;
using Project.Areas.Staff.Models.ViewModels;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Repositories.Interfaces;

namespace Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Bacsi, Yta")]
    [Route("don-thuoc")]
    public class PrescriptionsController : Controller
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IPrescriptionDetailRepository _prescriptionDetailRepository;
        private readonly IMedicineRepository _medicineRepository;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;

        public PrescriptionsController(
            IPrescriptionRepository prescriptionRepository,
            IPrescriptionDetailRepository prescriptionDetailRepository,
            IMedicineRepository medicineRepository,
            ITreatmentRecordRepository treatmentRecordRepository,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _prescriptionRepository = prescriptionRepository;
            _prescriptionDetailRepository = prescriptionDetailRepository;
            _medicineRepository = medicineRepository;
            _treatmentRecordRepository = treatmentRecordRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _prescriptionRepository.GetAllAdvancedAsync();
            var viewModelList = _mapper.Map<List<PrescriptionViewModel>>(list);
            viewModelList = viewModelList.OrderBy(x => x.PrescriptionDate).ToList();
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
            return View(prescription);
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
