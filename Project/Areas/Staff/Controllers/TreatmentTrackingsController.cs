using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using AutoMapper;
using Project.Areas.Staff.Models.Entities;
using Project.Areas.Staff.Models.DTOs.TrackingDTO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Project.Areas.Staff.Models.ViewModels;
using Project.Helpers;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Admin, Nhanvien")]
    public class TreatmentTrackingsController : Controller
    {
        private readonly ITreatmentTrackingRepository _treatmentTrackingRepository;
        public readonly ViewBagHelper _viewBagHelper;
        private readonly IMapper _mapper;
        public TreatmentTrackingsController
        (
            ITreatmentTrackingRepository treatmentTrackingRepository,
            ViewBagHelper viewBagHelper,
            IMapper mapper
        )
        {
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _viewBagHelper = viewBagHelper;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index()
        {
            var list = await _treatmentTrackingRepository.GetAllAdvancedAsync();
            var activeList = list.Where(x => x.IsActive).ToList();
            var viewModelList = _mapper.Map<List<TreatmentTrackingViewModel>>(activeList);
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(viewModelList);
        }
    }
}