using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.DTOs;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Admin.Models.ViewModels;
using Project.Helpers;
using Project.Repositories.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RoomsController : Controller
    {
        private readonly IRoomRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly ViewBagHelper _viewBagHelper;
        private readonly CodeGeneratorHelper _codeGenerator;

        public RoomsController
        (
            IRoomRepository repository,
            IEmployeeRepository employeeRepository,
            IMapper mapper,
            ViewBagHelper viewBagHelper,
            CodeGeneratorHelper codeGenerator
        )
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _viewBagHelper = viewBagHelper;
            _codeGenerator = codeGenerator;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _repository.GetAllAdvancedAsync();
            var activeList = list.Where(x => x.IsActive == true).ToList();
            var viewModelList = _mapper.Map<List<RoomViewModel>>(activeList);
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(viewModelList);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var entity = await _repository.GetByIdAdvancedAsync(id);
            if (entity == null)
            {
                return NotFound();
            }
            return View(entity);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await _viewBagHelper.BaseViewBag(ViewData);

            var model = new RoomDto
            {
                Code = await _codeGenerator.GenerateUniqueCodeAsync(_repository)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] RoomDto inputDto)
        {
            try
            {
                var entity = _mapper.Map<Room>(inputDto);
                entity.CreatedBy = "Admin";
                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;

                await _repository.CreateAsync(entity);
                return Json(new { success = true, message = "Thêm phòng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm phòng: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var entity = await _repository.GetByIdAdvancedAsync(id);
            if (entity == null) return NotFound();
            var dto = _mapper.Map<RoomDto>(entity);

            ViewBag.RoomId = entity.Id;

            await _viewBagHelper.BaseViewBag(ViewData);

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] RoomDto inputDto, Guid Id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(Id);
                if (entity == null) return NotFound();

                _mapper.Map(inputDto, entity);
                entity.UpdatedBy = "Admin";
                entity.UpdatedDate = DateTime.UtcNow;

                await _repository.UpdateAsync(entity);
                return Json(new { success = true, message = "Cập nhật phòng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật phòng: " + ex });
            }
        }

        public async Task<IActionResult> Trash()
        {
            var list = await _repository.GetAllAdvancedAsync();
            var activeList = list.Where(x => x.IsActive == false).ToList();
            var viewModelList = _mapper.Map<List<RoomViewModel>>(activeList);
            await _viewBagHelper.BaseViewBag(ViewData);
            return View(viewModelList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] string selectedIds)
        {
            var ids = new List<Guid>();
            foreach (var id in selectedIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (Guid.TryParse(id, out var parsedId))
                {
                    ids.Add(parsedId);
                }
            }

            if (_repository == null)
            {
                TempData["ErrorMessage"] = "Hệ thống gặp lỗi, vui lòng thử lại sau.";
                return RedirectToAction("Trash");
            }

            var rooms = new List<Room>();
            foreach (var id in ids)
            {
                var room = await _repository.GetByIdAsync(id);
                if (room == null) continue;
                var e = await _employeeRepository.GetAllAdvancedAsync();
                var hasEmployees = e.Any(x => x.RoomId == id);
                if (hasEmployees)
                {
                    rooms.Add(room);
                }
            }

            if (rooms.Any())
            {
                var names = string.Join(", ", rooms.Select(c => $"\"{c.Name}\""));
                var message = rooms.Count == 1
                    ? $"Không thể xóa phòng {names} vì vẫn còn nhân sự đang làm việc trong phòng này."
                    : $"Không thể xóa các phòng: {names} vì vẫn còn nhân sự đang làm việc trong phòng này.";
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Trash");
            }

            var delList = new List<Room>();
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
                    ? $"Đã xóa vĩnh viễn phòng {names} thành công"
                    : $"Đã xóa vĩnh viễn các phòng: {names} thành công";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng nào để xóa.";
            }

            return RedirectToAction("Trash");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToTrash([FromForm] string selectedIds)
        {
            var ids = new List<Guid>();
            foreach (var id in selectedIds.Split(','))
            {
                if (Guid.TryParse(id, out var parsedId))
                {
                    ids.Add(parsedId);
                }
            }

            var movedList = new List<Room>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = false;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _repository.UpdateAsync(entity);
                    movedList.Add(entity);
                }
            }

            if (movedList.Any())
            {
                var names = string.Join(", ", movedList.Select(c => $"\"{c.Name}\""));
                var message = movedList.Count == 1
                    ? $"Đã đưa phòng {names} thành công vào thùng rác"
                    : $"Đã đưa các phòng: {names} thành công vào thùng rác";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng nào để đưa vào thùng rác.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore([FromForm] string selectedIds)
        {
            var ids = new List<Guid>();
            foreach (var id in selectedIds.Split(','))
            {
                if (Guid.TryParse(id, out var parsedId))
                {
                    ids.Add(parsedId);
                }
            }
            var restoredList = new List<Room>();
            foreach (var id in ids)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    entity.IsActive = true;
                    entity.UpdatedBy = "Admin";
                    entity.UpdatedDate = DateTime.UtcNow;
                    await _repository.UpdateAsync(entity);
                    restoredList.Add(entity);
                }
            }

            if (restoredList.Any())
            {
                var names = string.Join(", ", restoredList.Select(c => $"\"{c.Name}\""));
                var message = restoredList.Count == 1
                    ? $"Đã khôi phục phòng {names} thành công."
                    : $"Đã khôi phục các phòng: {names} thành công.";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng nào để khôi phục.";
            }

            return RedirectToAction("Trash");
        }
    }
}
