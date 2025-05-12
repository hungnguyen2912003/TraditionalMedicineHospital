using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Helpers;
using Project.Repositories.Interfaces;

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
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;

        public HomeController
        (   
            IHealthInsuranceRepository healthInsuranceRepository, 
            IPatientRepository patientRepository, 
            IMapper mapper, ViewBagHelper viewBagHelper, 
            ITreatmentRecordRepository treatmentRecordRepository, 
            ITreatmentTrackingRepository treatmentTrackingRepository, 
            ITreatmentRecordDetailRepository treatmentRecordDetailRepository
        )
        {
            _healthInsuranceRepository = healthInsuranceRepository;
            _patientRepository = patientRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _treatmentRecordRepository = treatmentRecordRepository;
            _treatmentTrackingRepository = treatmentTrackingRepository;
            _treatmentRecordDetailRepository = treatmentRecordDetailRepository;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
