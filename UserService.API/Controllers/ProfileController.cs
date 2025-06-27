using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.API.DTOs;
using UserService.API.Services;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("UserService/[controller]/[action]")]
    public class ProfileController : ControllerBase
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly ProfileService _profileService;

        public ProfileController(ILogger<ProfileController> logger, ProfileService profileService)
        {
            _profileService = profileService;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = "Access")]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var result = await _profileService.GetProfileAsync();

            _logger.LogInformation("Ответ успешно отправлен");

            return result;
        }

        [Authorize(AuthenticationSchemes = "Access")]
        [HttpPost]
        public async Task<IActionResult> AddDataProfile(AddDataProfileJson json)
        {
            var result = await _profileService.AddDataProfileAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");

            return result;
        }

        [Authorize(AuthenticationSchemes = "Access")]
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(AddDataProfileJson json)
        {
            var result = await _profileService.UpdateProfileAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");

            return result;
        }

        [Authorize(AuthenticationSchemes = "Access")]
        [HttpPatch]
        public async Task<IActionResult> ClearProfile()
        {
            var result = await _profileService.ClearProfileAsync();

            _logger.LogInformation("Ответ успешно отправлен");

            return result;
        }
    }
}
