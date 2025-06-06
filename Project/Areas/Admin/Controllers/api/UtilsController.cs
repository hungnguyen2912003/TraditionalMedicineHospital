using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.BacSi.Models.DTOs;
using Project.Repositories.Interfaces;

namespace Project.Areas.Admin.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UtilsController : ControllerBase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly ITreatmentMethodRepository _treatmentMethodRepository;
        private readonly ITreatmentRecordDetailRepository _treatmentRecordDetailRepository;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public UtilsController(
            IRoomRepository roomRepository,
            ITreatmentMethodRepository treatmentMethodRepository,
            ITreatmentRecordDetailRepository treatmentRecordDetailRepository,
            IAssignmentRepository assignmentRepository,
            IEmployeeRepository employeeRepository)
        {
            _roomRepository = roomRepository;
            _treatmentMethodRepository = treatmentMethodRepository;
            _treatmentRecordDetailRepository = treatmentRecordDetailRepository;
            _assignmentRepository = assignmentRepository;
            _employeeRepository = employeeRepository;
        }

        /////////////////////////////////////////////////////////////////////////////////
        // Rooms
        [HttpGet("GetRoomsByDepartment/{id}")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRoomsByDepartment(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Mã khoa không hợp lệ");
            }

            try
            {
                var rooms = await _roomRepository.GetRoomsByDepartmentAsync(id);
                var roomList = rooms.Select(r => new { id = r.Id, name = r.Name }).ToList();
                return Ok(roomList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("GetRoomsByTreatmentMethod/{id}")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRoomsByTreatmentMethod(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Mã phương pháp điều trị không hợp lệ");
            }

            try
            {
                var rooms = await _roomRepository.GetRoomsByTreatmentMethodAsync(id);
                if (rooms == null || !rooms.Any())
                {
                    return NotFound("No rooms found for the specified treatment method");
                }

                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        // Treatment methods
        [HttpGet("GetTreatmentMethodsByDepartment/{depId}")]
        public async Task<ActionResult<IEnumerable<TreatmentMethod>>> GetTreatmentMethodsByDepartment(Guid depId)
        {
            if (depId == Guid.Empty)
            {
                return BadRequest("Mã khoa không hợp lệ");
            }

            try
            {
                var methods = await _treatmentMethodRepository.GetAllByDepartmentAsync(depId);
                return Ok(methods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("GetAvailableTreatmentMethods/{treatmentRecordId}")]
        public async Task<ActionResult<IEnumerable<TreatmentMethod>>> GetAvailableTreatmentMethods(Guid treatmentRecordId)
        {
            if (treatmentRecordId == Guid.Empty)
            {
                return BadRequest("Mã bản ghi điều trị không hợp lệ");
            }

            try
            {
                var methods = await _treatmentMethodRepository.GetAvailableTreatmentMethodsAsync(treatmentRecordId);
                return Ok(methods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Treatment details
        [HttpGet("GetTreatmentDetail/{code}")]
        public async Task<ActionResult<TreatmentRecordDetailDto>> GetTreatmentDetail(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Mã chi tiết bản ghi điều trị không hợp lệ");
            }

            try
            {
                var detail = await _treatmentRecordDetailRepository.GetDetailWithNamesAsync(code);
                if (detail == null)
                {
                    return NotFound($"Chi tiết bản ghi điều trị với mã {code} không tồn tại");
                }

                return Ok(detail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("UpdateTreatmentDetail")]
        public async Task<IActionResult> UpdateTreatmentDetail([FromBody] TreatmentRecordDetailUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var detail = await _treatmentRecordDetailRepository.GetByCodeAsync(model.Code);
                if (detail == null)
                {
                    return NotFound($"Chi tiết bản ghi điều trị với mã {model.Code} không tồn tại");
                }

                detail.RoomId = model.RoomId;
                detail.Note = model.Note;

                await _treatmentRecordDetailRepository.UpdateAsync(detail);

                var updatedDetail = await _treatmentRecordDetailRepository.GetDetailWithNamesAsync(model.Code);
                return Ok(updatedDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Assignments
        [HttpGet("GetAssignment/{code}")]
        public async Task<ActionResult<AssignmentDto>> GetAssignment(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Mã phân công không hợp lệ");
            }

            try
            {
                var assignment = await _assignmentRepository.GetByCodeAsync(code);
                if (assignment == null)
                {
                    return NotFound($"Phân công với mã {code} không tồn tại");
                }

                var dto = new AssignmentDto
                {
                    Code = assignment.Code,
                    DoctorName = assignment.Employee.Name,
                    StartDate = assignment.StartDate,
                    EndDate = assignment.EndDate,
                    Note = assignment.Note
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("UpdateAssignment")]
        public async Task<ActionResult<AssignmentDto>> UpdateAssignment([FromBody] AssignmentUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var assignment = await _assignmentRepository.GetByCodeAsync(model.Code);
                if (assignment == null)
                {
                    return NotFound($"Phân công với mã {model.Code} không tồn tại");
                }

                // Parse dates
                if (DateTime.TryParseExact(model.StartDate.ToString(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
                {
                    assignment.StartDate = startDate;
                }
                else
                {
                    return BadRequest("Ngày bắt đầu không hợp lệ");
                }

                if (DateTime.TryParseExact(model.EndDate.ToString(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
                {
                    assignment.EndDate = endDate;
                }
                else
                {
                    return BadRequest("Ngày kết thúc không hợp lệ");
                }

                assignment.Note = model.Note;

                await _assignmentRepository.UpdateAsync(assignment);

                var dto = new AssignmentDto
                {
                    Code = assignment.Code,
                    DoctorName = assignment.Employee.Name,
                    StartDate = assignment.StartDate,
                    EndDate = assignment.EndDate,
                    Note = assignment.Note
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}