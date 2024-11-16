using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.API.DTOs;
using UserService.API.Repositories;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("UserServise/[controller]/[action]")]
    public class ProfileController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(UserDbContext context, ILogger<ProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = "Access")]
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var user_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (user_id == null)
                {
                    _logger.LogWarning("Пользователь не найден. Пользователь ID не найден в claims.");
                    return NotFound("Пользователь не найден");
                }

                var user = await _context.UserProfiles
                .Include(u => u.User)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == user_id);

                if (user == null)
                {
                    _logger.LogWarning("Профиль пользователя с ID: {UserId} не найден", user_id);
                    return BadRequest("Пользователя не существует");
                }

                _logger.LogInformation("Профиль пользователя с ID: {UserId} успешно получен", user_id);

                return Ok(new GetProfileJson()
                {
                    Email = user.User.Email,
                    FirstName = user?.FirstName,
                    LastName = user?.LastName,
                    PhoneNumber = user?.PhoneNumber,
                    Address = user?.Address
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных пользователя с email: {UserEmail}");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте снова.");
            }

        }
        [Authorize(AuthenticationSchemes = "Access")]
        [HttpPost]
        public async Task<IActionResult> AddDataProfile(AddDataProfileJson json)
        {
            try
            {
                var user_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (user_id == null)
                {
                    _logger.LogWarning("Пользователь не найден. Пользователь ID не найден в claims.");
                    return NotFound("Пользователь не найден");
                }
                if (!int.TryParse(user_id, out int userId))
                {
                    _logger.LogWarning("Неверный идентификатор пользователя: {UserId}", user_id);
                    return BadRequest("Неверный идентификатор пользователя");
                }

                _logger.LogInformation("Запрос на обновление профиля пользователя с ID: {UserId}", userId);

                var userprofile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);

                if (userprofile == null)
                {
                    _logger.LogWarning("Профиль пользователя с ID: {UserId} не найден", userId);
                    return BadRequest("Пользователя не существует");
                }

                userprofile.FirstName = json?.FirstName ?? userprofile.FirstName;
                userprofile.LastName = json?.LastName ?? userprofile.LastName;
                userprofile.PhoneNumber = json?.PhoneNumber ?? userprofile.PhoneNumber;
                userprofile.Address = json?.Address ?? userprofile.Address;


                await _context.SaveChangesAsync();

                _logger.LogInformation("Профиль пользователя с ID: {UserId} успешно создан", userId);

                return Ok("Профиль успешно создан");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных пользователя с email: {UserEmail}");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте снова.");
            }
        }

    }
}
