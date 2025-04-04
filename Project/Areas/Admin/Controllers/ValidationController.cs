using Microsoft.AspNetCore.Mvc;
using Project.Areas.Admin.Models.Entities;
using Project.Services;

namespace Project.Areas.Admin.Controllers
{
    [Route("api/validation/{entityType}")]
    [ApiController]
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
                _ => throw new ArgumentException($"Invalid entity type: {entityType}")
            };

            var service = _serviceProvider.GetService(serviceType) as IBaseService;
            return service ?? throw new InvalidOperationException($"Service for {entityType} is not registered.");
        }
    }
}
