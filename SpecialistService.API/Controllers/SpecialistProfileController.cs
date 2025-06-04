using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpecialistService.API.DTOs;
using SpecialistService.API.Services;

namespace SpecialistService.API.Controllers
{
    [ApiController]
    [Route("SpecialistService/[controller]/[action]")]
    public class SpecialistProfileController : ControllerBase
    {
        private readonly ILogger<SpecialistProfileController> _logger;
        private readonly ProfileService _profileService;

        public SpecialistProfileController(ILogger<SpecialistProfileController> logger, ProfileService profileService)
        {
            _profileService = profileService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access")]
        public async Task<IActionResult> AllProfilesSpec()
        {
            List<GetProfileJson>? result = await _profileService.GetAllProfiles();

            if (result == null) return NotFound("Нет данных");

            _logger.LogInformation("Ответ успешно отправлен");

            return Ok(result);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> ProfileSpec()
        {
            GetProfileJson? result = await _profileService.GetProfileSpecialist();

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> AddDataProfileSpec(ProfileJson json)
        {
            string? result = await _profileService.AddNewData(json);

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> UpdateDataProfileSpec(UpdateProfileJson json)
        {
            string? result = await _profileService.ChangeInfoProfile(json);

            _logger.LogInformation("Ответ успешно отправлен");

            return Ok(result);
        }

        [HttpPatch]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> ClearProfileSpec()
        {
            var result = await _profileService.ClearProfile();

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }
    }
}
