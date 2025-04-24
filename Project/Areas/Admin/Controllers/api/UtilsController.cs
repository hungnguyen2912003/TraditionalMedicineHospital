using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
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

        public UtilsController(
            IRoomRepository roomRepository,
            ITreatmentMethodRepository treatmentMethodRepository)
        {
            _roomRepository = roomRepository;
            _treatmentMethodRepository = treatmentMethodRepository;
        }

        [HttpGet("GetAvailableTreatmentMethods")]
        public async Task<ActionResult<IEnumerable<TreatmentMethod>>> GetAvailableTreatmentMethods(Guid treatmentRecordId)
        {
            if (treatmentRecordId == Guid.Empty)
            {
                return BadRequest("Invalid treatment record ID");
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

        [HttpGet("GetRoomsByTreatmentMethod")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRoomsByTreatmentMethod(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid treatment method ID");
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
    }
} 