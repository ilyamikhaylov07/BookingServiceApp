using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.API.Controllers;
using UserService.API.DTOs;
using UserService.API.Repositories;

namespace UserService.API.Services
{
    public class ProfileService
    {
        private readonly UserDbContext _context;
        private readonly ILogger<ProfileController> _logger;
        private readonly IHttpContextAccessor _accessor;

        public ProfileService(UserDbContext context, ILogger<ProfileController> logger, IHttpContextAccessor accessor)
        {
            _context = context;
            _logger = logger;
            _accessor = accessor;
        }

        public async Task<IActionResult> GetProfileAsync()
        {
            var user_id = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user_id == null)
            {
                _logger.LogWarning("Пользователь не найден. Пользователь ID не найден в claims.");
                return new NotFoundObjectResult("Пользователь не найден");
            }

            var user = await _context.UserProfiles
            .Include(u => u.User)
            .FirstOrDefaultAsync(u => u.UserId.ToString() == user_id);

            if (user == null)
            {
                _logger.LogWarning("Профиль пользователя с ID: {UserId} не найден", user_id);
                return new BadRequestObjectResult("Пользователя не существует");
            }

            _logger.LogDebug("Профиль пользователя с ID: {UserId} успешно получен", user_id);

            return new OkObjectResult(new GetProfileJson()
            {
                Email = user.User.Email,
                FirstName = user?.FirstName,
                LastName = user?.LastName,
                PhoneNumber = user?.PhoneNumber,
                Address = user?.Address
            });

        }

        public async Task<IActionResult> AddDataProfileAsync(AddDataProfileJson json)
        {
            var user_id = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user_id == null)
            {
                _logger.LogWarning("Пользователь не найден. Пользователь ID не найден в claims.");
                return new NotFoundObjectResult("Пользователь не найден");
            }
            if (!int.TryParse(user_id, out int userId))
            {
                _logger.LogWarning("Не удаётся преобразовать в INT: {UserId}", user_id);
                return new BadRequestObjectResult("Неверный идентификатор пользователя");
            }

            _logger.LogDebug("Запрос на обновление профиля пользователя с ID: {UserId}", userId);

            var userprofile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);

            if (userprofile == null)
            {
                _logger.LogWarning("Профиль пользователя с ID: {UserId} не найден", userId);
                return new BadRequestObjectResult("Пользователя не существует");
            }

            userprofile.FirstName = json?.FirstName ?? userprofile.FirstName;
            userprofile.LastName = json?.LastName ?? userprofile.LastName;
            userprofile.PhoneNumber = json?.PhoneNumber ?? userprofile.PhoneNumber;
            userprofile.Address = json?.Address ?? userprofile.Address;

            await _context.SaveChangesAsync();

            _logger.LogDebug("Профиль пользователя с ID: {UserId} успешно создан", userId);

            return new OkObjectResult("Профиль успешно создан");
        }

        public async Task<IActionResult> UpdateProfileAsync(AddDataProfileJson json)
        {
            var user_id = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user_id == null)
            {
                _logger.LogWarning("Пользователь не найден. Пользователь ID не найден в claims.");
                return new NotFoundObjectResult("Пользователь не найден");
            }
            if (!int.TryParse(user_id, out int userId))
            {
                _logger.LogWarning("Не удаётся преобразовать в INT: {UserId}", user_id);
                return new BadRequestObjectResult("Неверный идентификатор пользователя");
            }

            _logger.LogDebug("Запрос на обновление профиля пользователя с ID: {UserId}", userId);

            var userprofile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);

            if (userprofile == null)
            {
                _logger.LogWarning("Профиль пользователя с ID: {UserId} не найден", userId);
                return new BadRequestObjectResult("Пользователя не существует");
            }

            userprofile.FirstName = json?.FirstName ?? userprofile.FirstName;
            userprofile.LastName = json?.LastName ?? userprofile.LastName;
            userprofile.PhoneNumber = json?.PhoneNumber ?? userprofile.PhoneNumber;
            userprofile.Address = json?.Address ?? userprofile.Address;

            await _context.SaveChangesAsync();

            _logger.LogDebug("Профиль пользователя с ID: {UserId} успешно обновлён", userId);

            return new OkObjectResult("Профиль успешно обновлён");
        }

        public async Task<IActionResult> ClearProfileAsync()
        {
            var user_id = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user_id == null)
            {
                _logger.LogWarning("Попытка очистки данных профиля без указания ID пользователя.");
                return new NotFoundObjectResult("Пользователь не найден");
            }

            if (!int.TryParse(user_id, out int userId))
            {
                _logger.LogWarning("Не удаётся преобразовать в INT: {UserId}", user_id);
                return new BadRequestObjectResult("Неверный идентификатор пользователя");
            }

            _logger.LogDebug("Запрос на очистку необязательных данных профиля пользователя с ID: {UserId}", userId);

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userProfile == null)
            {
                _logger.LogWarning("Профиль пользователя с ID: {UserId} не найден", userId);
                return new NotFoundObjectResult("Профиль пользователя не существует");
            }

            userProfile.FirstName = null;
            userProfile.LastName = null;
            userProfile.PhoneNumber = null;
            userProfile.Address = null;

            await _context.SaveChangesAsync();

            _logger.LogDebug("Необязательные данные профиля пользователя с ID: {UserId} успешно очищены", userId);
            return new OkObjectResult("Необязательные данные профиля успешно очищены");
        }
    }
}
