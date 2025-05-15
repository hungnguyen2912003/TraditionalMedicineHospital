using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using Project.Areas.Staff.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Project.Helpers;
using Project.Areas.Staff.Models.Entities;
using Project.Models.Enums;

namespace Project.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Bacsi")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;

        public PaymentsController
        (   
            IPaymentRepository paymentRepository,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator,
            ITreatmentRecordRepository treatmentRecordRepository
        )
        {
            _paymentRepository = paymentRepository;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
            _treatmentRecordRepository = treatmentRecordRepository;
        }

        public async Task<IActionResult> Index()
        {
            var payments = await _paymentRepository.GetAllAdvancedAsync();
            var viewModels = payments.Select(p => {
                var tr = p.TreatmentRecord;
                var totalPrescriptionCost = tr.Prescriptions?.Sum(pre => pre.TotalCost) ?? 0;

                decimal totalTreatmentMethodCost = 0;
                foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
                {
                    var room = detail.Room;
                    var method = room?.TreatmentMethod;
                    if (method == null) continue;
                    int count = detail.TreatmentTrackings?.Count(t => t.Status == TrackingStatus.CoDieuTri) ?? 0;
                    totalTreatmentMethodCost += method.Cost * count;
                }

                decimal totalCostBeforeInsurance = totalPrescriptionCost + totalTreatmentMethodCost;
                decimal insuranceAmount = 0;
                var hi = tr.Patient?.HealthInsurance;
                if (hi != null)
                {
                    if (hi.IsRightRoute)
                        insuranceAmount = totalCostBeforeInsurance * 0.8m;
                    else
                        insuranceAmount = totalCostBeforeInsurance * 0.6m;
                }

                decimal finalCost = totalCostBeforeInsurance - insuranceAmount - tr.AdvancePayment;
                if (finalCost < 0) finalCost = 0;

                return new PaymentViewModel
                {
                    Id = p.Id,
                    Code = p.Code,
                    PaymentDate = p.PaymentDate,
                    TreatmentRecordCode = tr.Code,
                    PatientName = tr.Patient!.Name,
                    TotalPrescriptionCost = totalPrescriptionCost,
                    TotalTreatmentMethodCost = totalTreatmentMethodCost,
                    InsuranceAmount = insuranceAmount,
                    TotalCost = totalCostBeforeInsurance,
                    IsActive = p.IsActive,
                    Status = p.Status == PaymentStatus.DaThanhToan ? PaymentStatus.DaThanhToan : PaymentStatus.ChuaThanhToan
                };
            });

            // Lấy danh sách bệnh nhân
            var patients = (await _treatmentRecordRepository.GetAllAdvancedAsync())
                .Select(tr => tr.Patient)
                .Distinct()
                .Select(p => new {
                    id = p.Id,
                    name = p.Name
                }).ToList();
            ViewBag.Patients = patients;

            // Lấy toàn bộ TreatmentRecord (Id, Code, PatientId, Status, StartDate, EndDate)
            var treatmentRecords = (await _treatmentRecordRepository.GetAllAdvancedAsync())
                .Select(tr => new {
                    id = tr.Id,
                    code = tr.Code,
                    patientId = tr.Patient.Id,
                    status = tr.Status,
                    startDate = tr.StartDate,
                    endDate = tr.EndDate
                }).ToList();
            ViewBag.TreatmentRecordsCanPayment = treatmentRecords;

            ViewBag.PaymentCode = await _codeGenerator.GenerateUniqueCodeAsync(_paymentRepository);

            return View(viewModels);
        }
    
    
        public async Task<IActionResult> Details(Guid id)
        {
            var payment = await _paymentRepository.GetByIdAdvancedAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            var tr = payment.TreatmentRecord;
            var totalPrescriptionCost = tr.Prescriptions?.Sum(pre => pre.TotalCost) ?? 0;

            decimal totalTreatmentMethodCost = 0;
            foreach (var detail in tr.TreatmentRecordDetails ?? new List<TreatmentRecordDetail>())
            {
                var room = detail.Room;
                var method = room?.TreatmentMethod;
                if (method == null) continue;
                int count = detail.TreatmentTrackings?.Count(t => t.Status == TrackingStatus.CoDieuTri) ?? 0;
                totalTreatmentMethodCost += method.Cost * count;
            }

            decimal totalCostBeforeInsurance = totalPrescriptionCost + totalTreatmentMethodCost;
            decimal insuranceAmount = 0;
            var hi = tr.Patient?.HealthInsurance;
            if (hi != null)
            {
                if (hi.IsRightRoute)
                    insuranceAmount = totalCostBeforeInsurance * 0.8m;
                else
                    insuranceAmount = totalCostBeforeInsurance * 0.6m;
            }

            decimal finalCost = totalCostBeforeInsurance - insuranceAmount - tr.AdvancePayment;
            if (finalCost < 0) finalCost = 0;

            var viewModel = new PaymentViewModel
            {
                Id = payment.Id,
                Code = payment.Code,
                Note = payment.Note,
                PaymentDate = payment.PaymentDate,
                CreatedDate = payment.CreatedDate,
                CreatedBy = payment.CreatedBy,
                UpdatedDate = payment.UpdatedDate,
                UpdatedBy = payment.UpdatedBy,
                TreatmentRecordCode = tr.Code,
                PatientName = tr.Patient!.Name,
                TotalPrescriptionCost = totalPrescriptionCost,
                TotalTreatmentMethodCost = totalTreatmentMethodCost,
                InsuranceAmount = insuranceAmount,
                AdvancePayment = tr.AdvancePayment,
                TotalCost = totalCostBeforeInsurance,
                FinalCost = finalCost,
                IsActive = payment.IsActive,
                Status = payment.Status,
                Prescriptions = tr.Prescriptions?.ToList() ?? new(),
                PatientHealthInsuranceNumber = hi?.Number,
                PatientHealthInsuranceExpiredDate = hi?.ExpiryDate,
                PatientHealthInsurancePlaceOfRegistration = hi?.PlaceOfRegistration,
                PatientHealthInsuranceIsRightRoute = hi?.IsRightRoute,
                TreatmentDetails = tr.TreatmentRecordDetails?.ToList() ?? new()
            };

            return View(viewModel);
        }
    }
}
