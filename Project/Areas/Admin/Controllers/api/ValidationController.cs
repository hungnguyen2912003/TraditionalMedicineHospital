using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("check")]
        public async Task<IActionResult> Check(string entityType, string type, string value, Guid? id = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
                {
                    return BadRequest(new { success = false, message = "Missing required parameters." });
                }

                if (!EntityServiceMap.ServiceTypes.TryGetValue(entityType, out var serviceType))
                {
                    return BadRequest(new { success = false, message = $"Invalid entity type: {entityType}" });
                }

                var service = _serviceProvider.GetService(serviceType) as IBaseService;
                if (service == null)
                {
                    return StatusCode(500, new { success = false, message = $"Service for {entityType} is not registered." });
                }

                bool isUnique = type.ToLower() switch
                {
                    "code" => await service.IsCodeUniqueAsync(value, id),
                    "name" => await service.IsNameUniqueAsync(value, id),
                    "number" => await service.IsNumberUniqueAsync(value, id),
                    "email" => await service.IsEmailUniqueAsync(value, id),
                    "phone" => await service.IsPhoneUniqueAsync(value, id),
                    "identitynumber" => await service.IsIdentityNumberUniqueAsync(value, id),
                    _ => throw new ArgumentException($"Invalid check type: {type}")
                };

                return Ok(new { success = true, isUnique = isUnique });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
