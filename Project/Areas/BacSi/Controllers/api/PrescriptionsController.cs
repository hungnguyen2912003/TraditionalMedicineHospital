using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.BacSi.Models.DTOs;
using Project.Helpers;
using Project.Repositories.Interfaces;
using Project.Services.Features;
using Repositories.Interfaces;
using Project.Models.Enums;

namespace Project.Areas.Staff.Controllers.api
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IPrescriptionDetailRepository _prescriptionDetailRepository;
        private readonly IMapper _mapper;
        private readonly JwtManager _jwtManager;
        private readonly IUserRepository _userRepository;
        private readonly CodeGeneratorHelper _codeGenerator;
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;

        public PrescriptionsController(
            IPrescriptionRepository prescriptionRepository,
            IPrescriptionDetailRepository prescriptionDetailRepository,
            IMapper mapper,
            JwtManager jwtManager,
            IUserRepository userRepository,
            CodeGeneratorHelper codeGenerator,
            ITreatmentRecordRepository treatmentRecordRepository
        )
        {
            _prescriptionRepository = prescriptionRepository;
            _prescriptionDetailRepository = prescriptionDetailRepository;
            _mapper = mapper;
            _jwtManager = jwtManager;
            _userRepository = userRepository;
            _codeGenerator = codeGenerator;
            _treatmentRecordRepository = treatmentRecordRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PrescriptionCreateRequest request)
        {
            try
            {
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                    return Unauthorized("Chưa đăng nhập.");

                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                    return Unauthorized("Token không hợp lệ.");

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || user.Employee == null)
                    return NotFound("Không tìm thấy thông tin nhân viên.");

                var employee = user.Employee;

                var prescription = new Prescription
                {
                    Id = Guid.NewGuid(),
                    Code = await _codeGenerator.GenerateUniqueCodeAsync(_prescriptionRepository),
                    PrescriptionDate = request.PrescriptionDate,
                    Note = request.Note,
                    TreatmentRecordId = request.TreatmentRecordId,
                    EmployeeId = employee.Id,
                    CreatedBy = employee.Code,
                    CreatedDate = DateTime.Today
                };

                var details = new List<PrescriptionDetail>();
                foreach (var d in request.Details)
                {
                    var detail = new PrescriptionDetail
                    {
                        Id = Guid.NewGuid(),
                        PrescriptionId = prescription.Id,
                        MedicineId = d.MedicineId,
                        Quantity = d.Quantity,
                        CreatedBy = employee.Code,
                        CreatedDate = DateTime.Today
                    };
                    details.Add(detail);
                }

                // Lưu vào DB
                await _prescriptionRepository.CreateAsync(prescription);
                foreach (var detail in details)
                {
                    await _prescriptionDetailRepository.CreateAsync(detail);
                }

                return Ok(new { success = true, message = "Lưu đơn thuốc thành công!", prescriptionId = prescription.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PrescriptionCreateRequest request)
        {
            try
            {
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                    return Unauthorized("Chưa đăng nhập.");

                var (username, role) = _jwtManager.GetClaimsFromToken(token);
                if (string.IsNullOrEmpty(username))
                    return Unauthorized("Token không hợp lệ.");

                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null || user.Employee == null)
                    return NotFound("Không tìm thấy thông tin nhân viên.");

                var employee = user.Employee;

                var prescription = await _prescriptionRepository.GetByIdAdvancedAsync(id);
                if (prescription == null)
                    return NotFound("Không tìm thấy đơn thuốc.");

                // Cập nhật thông tin đơn thuốc
                prescription.PrescriptionDate = request.PrescriptionDate;
                prescription.Note = request.Note;
                prescription.TreatmentRecordId = request.TreatmentRecordId;
                prescription.UpdatedBy = employee.Code;
                prescription.UpdatedDate = DateTime.Today;

                // Xóa các PrescriptionDetail cũ
                var oldDetails = prescription.PrescriptionDetails.ToList();
                var newDetails = request.Details;
                foreach (var old in oldDetails)
                {
                    if (!newDetails.Any(nd => nd.MedicineId == old.MedicineId))
                    {
                        await _prescriptionDetailRepository.DeleteAsync(old.Id);
                    }
                }

                // Thêm mới và update
                foreach (var nd in newDetails)
                {
                    var old = oldDetails.FirstOrDefault(od => od.MedicineId == nd.MedicineId);
                    if (old == null)
                    {
                        // Thêm mới
                        var detail = new PrescriptionDetail
                        {
                            Id = Guid.NewGuid(),
                            PrescriptionId = prescription.Id,
                            MedicineId = nd.MedicineId,
                            Quantity = nd.Quantity,
                            CreatedBy = employee.Code,
                            CreatedDate = DateTime.Today,
                        };
                        await _prescriptionDetailRepository.CreateAsync(detail);
                    }
                    else
                    {
                        // Update nếu có thay đổi
                        if (old.Quantity != nd.Quantity)
                        {
                            old.Quantity = nd.Quantity;
                            old.UpdatedBy = employee.Code;
                            old.UpdatedDate = DateTime.Today;
                            await _prescriptionDetailRepository.UpdateAsync(old);
                        }
                    }
                }

                await _prescriptionRepository.UpdateAsync(prescription);

                return Ok(new { success = true, message = "Cập nhật đơn thuốc thành công!", prescriptionId = prescription.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var prescription = await _prescriptionRepository.GetByIdAdvancedAsync(id);
                if (prescription == null)
                    return NotFound("Không tìm thấy đơn thuốc.");

                // Lấy danh sách chi tiết đơn thuốc
                var details = prescription.PrescriptionDetails?.Select(d => new
                {
                    medicineId = d.MedicineId,
                    medicineName = d.Medicine?.Name,
                    quantity = d.Quantity,
                    unitPrice = d.Medicine?.Price ?? 0,
                    totalPrice = (d.Medicine?.Price ?? 0) * d.Quantity
                }).ToList();

                // Tính tổng tiền
                var totalCost = details?.Sum(d => d.totalPrice) ?? 0;

                // Trả về thông tin đơn thuốc và chi tiết
                return Ok(new
                {
                    id = prescription.Id,
                    code = prescription.Code,
                    prescriptionDate = prescription.PrescriptionDate,
                    note = prescription.Note,
                    treatmentRecordId = prescription.TreatmentRecordId,
                    treatmentRecordCode = prescription.TreatmentRecord?.Code,
                    patientName = prescription.TreatmentRecord?.Patient?.Name,
                    totalCost = totalCost,
                    details = details
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }

        [HttpGet("patients-with-active-treatment")]
        public async Task<IActionResult> GetPatientsWithActiveTreatment()
        {
            var treatmentRecords = (await _treatmentRecordRepository.GetAllAdvancedAsync())
                .Where(tr => tr.Status == TreatmentStatus.DangDieuTri)
                .ToList();

            var patients = treatmentRecords
                .Select(tr => new
                {
                    id = tr.Patient.Id,
                    name = tr.Patient.Name,
                    treatmentRecordId = tr.Id,
                    treatmentRecordCode = tr.Code
                })
                .Distinct()
                .ToList();

            return Ok(patients);
        }
    }
}