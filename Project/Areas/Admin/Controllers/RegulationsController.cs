﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Helpers;
using Project.Repositories.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("quy-dinh")]
    public class RegulationsController : Controller
    {
        private readonly IRegulationRepository _repository;
        private readonly ITreatmentRecordRegulationRepository _treatmentRecordRegulationRepository;
        private readonly IMapper _mapper;
        private readonly CodeGeneratorHelper _codeGenerator;

        public RegulationsController
        (
            IRegulationRepository repository,
            ITreatmentRecordRegulationRepository treatmentRecordRegulationRepository,
            IMapper mapper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _repository = repository;
            _treatmentRecordRegulationRepository = treatmentRecordRegulationRepository;
            _mapper = mapper;
            _codeGenerator = codeGenerator;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _repository.GetAllAsync();
            list = list.OrderBy(c => c.ExpirationDate).ToList();
            return View(list);
        }

        [HttpGet("chi-tiet/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();
            return View(entity);
        }

        [HttpGet("them-moi")]
        public async Task<IActionResult> Create()
        {
            var model = new RegulationDto
            {
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_repository)
            };
            return View(model);
        }

        [HttpPost("them-moi")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] RegulationDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<Regulation>(inputDto);

                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;

                await _repository.CreateAsync(entity);
                return Json(new { success = true, message = "Thêm quy định thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm quy định: " + ex.Message });
            }
        }

        [HttpGet("chinh-sua/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<RegulationDto>(entity);

            ViewBag.RegulationId = entity.Id;

            return View(dto);
        }

        [HttpPost("chinh-sua/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] RegulationDto inputDto, Guid Id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                await _repository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật quy định thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật quy định: " + ex.Message });
            }
        }

        [HttpPost("xoa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] string selectedIds)
        {
            var ids = new List<Guid>();
            foreach (var id in selectedIds.Split(','))
            {
                if (Guid.TryParse(id, out var parsedId))
                {
                    ids.Add(parsedId);
                }
            }

            // Lấy tất cả TreatmentRecord_Regulation có RegulationId thuộc ids
            var allTreatmentRecordRegulations = await _treatmentRecordRegulationRepository.GetAllAsync();
            var usedTreatmentRecordRegulations = allTreatmentRecordRegulations.Where(t => ids.Contains(t.RegulationId)).ToList();
            if (usedTreatmentRecordRegulations.Any())
            {
                var names = string.Join(", ", usedTreatmentRecordRegulations.Select(t => $"\"{t.Regulation.Name}\"").Distinct());
                var message = usedTreatmentRecordRegulations.Count == 1
                    ? $"Không thể xóa quy định {names} vì vẫn còn phiếu điều trị đang sử dụng quy định này."
                    : $"Không thể xóa các quy định đã chọn vì vẫn còn phiếu điều trị đang sử dụng các quy định này.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index");
            }

            var delList = new List<Regulation>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    await _repository.DeleteAsync(id);
                    delList.Add(entity);
                }
            }

            if (delList.Any())
            {
                var names = string.Join(", ", delList.Select(c => $"\"{c.Name}\""));
                var message = delList.Count == 1
                    ? $"Đã xóa quy định {names} thành công"
                    : $"Đã xóa các quy định đã chọn thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy quy định nào để xóa.";
            }

            return RedirectToAction("Index");
        }
    }
}
