using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Project.Repositories.Interfaces;
using Project.Services.Interfaces;

namespace Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MedicinesController : Controller
    {
        private readonly IMedicineRepository _repository;
        private readonly IMapper _mapper;
        private readonly IImageService _service;
        public MedicinesController
        (
            IMedicineRepository repository,
            IMapper mapper,
            IImageService service
        )
        {
            _repository = repository;
            _mapper = mapper;
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _repository.GetAllAsync();
            var activeList = list.Where(x => x.Status == true).ToList();
            return View(activeList);
        }
    }
}
