using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using UserService.API.DTOs;
using UserService.API.Models;
using UserService.API.Repositories;
using UserService.API.Services;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("UserServise/[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly TokenManager _manager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserDbContext context, TokenManager token, ILogger<AuthController> logger)
        {
            _context = context;
            _manager = token;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationJson json)
        {
            _logger.LogInformation("Начало регистрации пользователя с email: {Email}", json.Email);
            try
            {
                string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
                bool isValidEmail = Regex.IsMatch(json.Email, pattern);

                if (!isValidEmail)
                {
                    _logger.LogWarning("Неверный формат email: {Email}", json.Email);
                    return BadRequest("Неверная почта");
                }

                if (json.Password.Length < 8)
                {
                    _logger.LogWarning("Пароль слишком короткий для пользователя: {Email}", json.Email);
                    return BadRequest("Пароль должен содержать от 8 символов");
                }

                int roleId = json.IsSpecialist ? 1 : 2;
                var role = await _context.Roles.FindAsync(roleId);

                if (role == null)
                {
                    _logger.LogError("Роль с ID {RoleId} не найдена", roleId);
                    return BadRequest("Роль не найдена");
                }

                var findUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == json.Email || u.Username == json.UserName);

                if (findUser != null)
                {
                    _logger.LogWarning("Пользователь с email {Email} уже существует", json.Email);
                    return BadRequest("Эта почта уже зарегистрирована");
                }

                var password = HasherPassword.HashPassword(json.Password); // Хэшируем пароль
                var people = new Users
                {
                    Username = json.UserName,
                    Email = json.Email,
                    PasswordHash = password,
                    Role = role,
                    Created = DateTime.UtcNow
                };
     
                _context.Users.Add(people);

                await _context.SaveChangesAsync();

                var profiles = new UserProfiles
                {
                    UserId = people.Id,
                    FirstName = null,
                    LastName = null,
                    PhoneNumber = null,
                    Address = null
                };

                _context.UserProfiles.Add(profiles);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Пользователь {Email} успешно зарегистрирован", json.Email);
                return Ok("Пользователь успешно зарегистрирован");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации пользователя: {Email}", json.Email);
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUserJson json)
        {
            _logger.LogInformation("Попытка входа пользователя с email: {Email}", json.Email);
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(e => e.Email == json.Email);

                if (user == null)
                {
                    _logger.LogWarning("Попытка входа с несуществующим email: {Email}", json.Email);
                    return BadRequest("Эта почта не зарегистрирована");
                }

                if (!HasherPassword.VerifyPassword(json.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Неверный пароль для пользователя с email: {Email}", json.Email);
                    return BadRequest("Неправильный пароль");
                }

                string role = user.RoleId == 1 ? "Specialist" : "User";
                _logger.LogInformation("Role: {role}:", role);

                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, role)
                };

                string accessToken = _manager.GenerateAccessToken(claims);
                string refreshToken = _manager.GenerateRefreshToken(claims);

                // Сохраняем refresh token и срок его действия в базе данных для пользователя
                user.RefreshToken = refreshToken;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Пользователь {Email} успешно вошел в систему", json.Email);
                return Ok(new TokenJson { AccessToken = accessToken, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при входе пользователя с email: {Email}", json.Email);
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateToken(RefreshTokenJson json)
        {
            _logger.LogInformation("Запрос обновления токена для refresh token: {RefreshToken}", json.RefreshToken);
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(r => r.RefreshToken == json.RefreshToken);

                if (user == null)
                {
                    _logger.LogWarning("Невалидный refresh token: {RefreshToken}", json.RefreshToken);
                    return Unauthorized("Invalid refresh token");
                }

                string role = user.RoleId == 1 ? "Specialist" : "User";

                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, role)
                };

                string accessToken = _manager.GenerateAccessToken(claims);
                string refreshToken = _manager.GenerateRefreshToken(claims);

                user.RefreshToken = refreshToken;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Токены для пользователя {Email} успешно обновлены", user.Email);
                return Ok(new TokenJson { AccessToken = accessToken, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении токена для refresh token: {RefreshToken}", json.RefreshToken);
                return BadRequest(ex.Message);
            }
        }
    }
}
