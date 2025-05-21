using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Staff.Models.ViewModels;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Repositories.Interfaces;

namespace Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Bacsi, Yta")]
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

        public async Task<IActionResult> Index()
        {
            var list = await _prescriptionRepository.GetAllAdvancedAsync();
            var viewModelList = _mapper.Map<List<PrescriptionViewModel>>(list);
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(viewModelList);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var prescription = await _prescriptionRepository.GetByIdAdvancedAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }
            return View(prescription);
        }
    }
}
