using Microsoft.AspNetCore.Mvc;
using Project.Areas.NhanVien.Models.DTOs;
using Project.Repositories.Interfaces;

namespace Project.Areas.NhanVien.Controllers.api
{
    [Area("NhanVien")]
    [Route("api/violated-patients")]
    [ApiController]
    public class ViolatedHandlesController : ControllerBase
    {
        private readonly ITreatmentRecordRepository _treatmentRecordRepository;
        public ViolatedHandlesController(ITreatmentRecordRepository treatmentRecordRepository)
        {
            _treatmentRecordRepository = treatmentRecordRepository;
        }

        [HttpPost("set-violated")]
        public async Task<IActionResult> SetViolated([FromBody] SetViolatedRequest req)
        {
            var record = await _treatmentRecordRepository.GetByIdAsync(req.TreatmentRecordId);
            if (record == null)
                return NotFound();
            record.IsViolated = true;
            await _treatmentRecordRepository.UpdateAsync(record);
            return Ok(new { success = true });
        }
    }
}