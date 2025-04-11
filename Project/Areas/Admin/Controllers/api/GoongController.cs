using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Project.Models;

namespace Project.Areas.Admin.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoongController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _goongApiKey;

        public GoongController(IHttpClientFactory httpClientFactory, IOptions<GoongSettings> goongSettings)
        {
            _httpClientFactory = httpClientFactory;
            _goongApiKey = goongSettings.Value.ApiKey;
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> Autocomplete(string input, string sessiontoken)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(sessiontoken))
            {
                return BadRequest("Input and sessiontoken are required.");
            }

            var client = _httpClientFactory.CreateClient();
            var url = $"https://rsapi.goong.io/Place/AutoComplete?api_key={_goongApiKey}&input={Uri.EscapeDataString(input)}&sessiontoken={sessiontoken}";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
