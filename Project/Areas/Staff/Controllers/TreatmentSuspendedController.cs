using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using Project.Areas.Staff.Models.ViewModels;
using Project.Models.Enums;
using System.Linq;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Route("dinh-chi-dieu-tri")]
    public class TreatmentSuspendedController : Controller
    {
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public TreatmentSuspendedController(ITreatmentRecordRepository treatmentRecordRepository, IEmployeeRepository employeeRepository)
        {
            _treatmentRecordRepository = treatmentRecordRepository;
            _employeeRepository = employeeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allRecords = await _treatmentRecordRepository.GetAllAdvancedAsync();
            var suspendedRecordsRaw = allRecords
                .Where(r => r.Status == TreatmentStatus.DaHoanThanh || r.Status == TreatmentStatus.DaHuyBo)
                .ToList();

            var suspendedByCodes = suspendedRecordsRaw
                .Where(r => !string.IsNullOrEmpty(r.SuspendedBy))
                .Select(r => r.SuspendedBy)
                .Distinct()
                .ToList();

            var employees = await _employeeRepository.GetByCodesAsync(suspendedByCodes!);
            var codeNameDict = employees.ToDictionary(e => e.Code, e => e.Name);

            var suspendedRecords = suspendedRecordsRaw
                .Select(r => new TreatmentSuspendedViewModel
                {
                    PatientName = r.Patient.Name,
                    TreatmentCode = r.Code,
                    SuspendedDate = r.SuspendedDate,
                    SuspendedBy = !string.IsNullOrEmpty(r.SuspendedBy) && codeNameDict.ContainsKey(r.SuspendedBy)
                        ? codeNameDict[r.SuspendedBy]
                        : r.SuspendedBy,
                    Reason = r.SuspendedReason
                })
                .ToList();
            return View(suspendedRecords);
        }
    }
}