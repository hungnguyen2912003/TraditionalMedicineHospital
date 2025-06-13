using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Project.Repositories.Interfaces;
using Project.Models.Enums;

namespace Project.Areas.NhanVien.Controllers
{
    [Area("NhanVien")]
    [Authorize(Roles = "NhanVienHanhChinh")]
    [Route("nhan-vien")]
    public class HomeController : Controller
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;

        public HomeController(IPaymentRepository paymentRepository, ITreatmentRecordRepository treatmentRecordRepository)
        {
            _paymentRepository = paymentRepository;
            _treatmentRecordRepository = treatmentRecordRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ TreatmentRecord chưa có tạm ứng (AdvancePayment == null)
            var allTreatmentRecords = await _treatmentRecordRepository.GetAllAdvancedAsync();
            var treatmentRecords = allTreatmentRecords
                .Where(tr => (tr.AdvancePayment == null || tr.AdvancePayment == 0) && tr.Status == TreatmentStatus.DangDieuTri)
                .Select(tr => new
                {
                    id = tr.Id,
                    code = tr.Code,
                    patientId = tr.Patient.Id,
                    status = tr.Status,
                    startDate = tr.StartDate,
                    endDate = tr.EndDate
                }).ToList();

            // Lấy danh sách bệnh nhân có thể tạm ứng (có ít nhất 1 TreatmentRecord chưa tạm ứng)
            var patients = allTreatmentRecords
                .Where(tr => (tr.AdvancePayment == null || tr.AdvancePayment == 0) && tr.Status == TreatmentStatus.DangDieuTri)
                .GroupBy(tr => tr.Patient.Id)
                .Select(g => new
                {
                    id = g.Key,
                    name = g.First().Patient.Name
                }).ToList();

            ViewBag.Patients = patients;
            ViewBag.TreatmentRecords = treatmentRecords;

            return View();
        }
    }
}
