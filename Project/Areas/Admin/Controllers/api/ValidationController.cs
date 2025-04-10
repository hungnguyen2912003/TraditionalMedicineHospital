﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Areas.Staff.Models.Entities;
using Project.Services;

namespace Project.Areas.Admin.Controllers.api
{
    [Route("api/validation/{entityType}")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ValidationController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpGet("check-code")]
        public async Task<IActionResult> CheckCodeExists(string entityType, string code, Guid? id = null)
        {
            var service = GetService(entityType);
            var isUnique = await service.IsCodeUniqueAsync(code, id);
            return Ok(isUnique);
        }

        [HttpGet("check-name")]
        public async Task<IActionResult> CheckNameExists(string entityType, string name, Guid? id = null)
        {
            var service = GetService(entityType);
            var isUnique = await service.IsNameUniqueAsync(name, id);
            return Ok(isUnique);
        }

        [HttpGet("check-number")]
        public async Task<IActionResult> CheckNumberExists(string entityType, string number, Guid? id = null)
        {
            var service = GetService(entityType);
            var isUnique = await service.IsNumberUniqueAsync(number, id);
            return Ok(isUnique);
        }

        private IBaseService GetService(string entityType)
        {
            Type serviceType = entityType.ToLower() switch
            {
                "medicine" => typeof(IBaseService<Medicine>),
                "medicinecategory" => typeof(IBaseService<MedicineCategory>),
                "department" => typeof(IBaseService<Department>),
                "employeecategory" => typeof(IBaseService<EmployeeCategory>),
                "employee" => typeof(IBaseService<Employee>),
                "treatment" => typeof(IBaseService<TreatmentMethod>),
                "room" => typeof(IBaseService<Room>),
                "regulation" => typeof(IBaseService<Regulation>),
                "patient" => typeof(IBaseService<Patient>),
                "treatmentrecord" => typeof(IBaseService<TreatmentRecord>),
                "healthinsurance" => typeof(IBaseService<HealthInsurance>),
                _ => throw new ArgumentException($"Invalid entity type: {entityType}")
            };

            var service = _serviceProvider.GetService(serviceType) as IBaseService;
            return service ?? throw new InvalidOperationException($"Service for {entityType} is not registered.");
        }
    }
}
