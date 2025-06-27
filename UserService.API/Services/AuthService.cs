using Infrastructure.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using UserService.API.Controllers;
using UserService.API.DTOs;
using UserService.API.Models;
using UserService.API.Repositories;

namespace UserService.API.Services
{
    public class AuthService
    {
        private readonly UserDbContext _context;
        private readonly TokenManager _manager;
        private readonly ILogger<AuthController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuthService(UserDbContext context, TokenManager token, ILogger<AuthController> logger, IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
            _context = context;
            _manager = token;
            _logger = logger;
        }

        public async Task<IActionResult> RegisterAsync(RegistrationJson json)
        {
            _logger.LogInformation("Начало регистрации пользователя с email: {Email}", json.Email);
            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
            bool isValidEmail = Regex.IsMatch(json.Email, pattern);

            if (!isValidEmail)
            {
                _logger.LogWarning("Неверный формат email: {Email}", json.Email);
                return new BadRequestObjectResult("Неверная почта");
            }

            if (json.Password.Length < 8)
            {
                _logger.LogWarning("Пароль слишком короткий для пользователя: {Email}", json.Email);
                return new BadRequestObjectResult("Пароль должен содержать от 8 символов");
            }

            int roleId = json.IsSpecialist ? 1 : 2;
            var role = await _context.Roles.FindAsync(roleId);

            if (role == null)
            {
                _logger.LogError("Роль с ID {RoleId} не найдена", roleId);
                return new BadRequestObjectResult("Роль не найдена");
            }

            var findUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == json.Email || u.Username == json.UserName);

            if (findUser != null)
            {
                _logger.LogWarning("Пользователь с email {Email} уже существует", json.Email);
                return new BadRequestObjectResult("Эта почта уже зарегистрирована");
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

            await PublishInRMQAsync(role, people);

            return new OkObjectResult("Пользователь успешно зарегистрирован");
        }


        public async Task<IActionResult> SignInAsync(LoginUserJson json)
        {
            var user = await _context.Users.FirstOrDefaultAsync(e => e.Email == json.Email);

            if (user == null)
            {
                _logger.LogWarning("Попытка входа с несуществующим email: {Email}", json.Email);
                return new BadRequestObjectResult("Эта почта не зарегистрирована");
            }

            if (!HasherPassword.VerifyPassword(json.Password, user.PasswordHash))
            {
                _logger.LogWarning("Неверный пароль для пользователя с email: {Email}", json.Email);
                return new BadRequestObjectResult("Неправильный пароль");
            }

            string role = user.RoleId == 1 ? "Specialist" : "User";
            _logger.LogDebug("Role: {role}:", role);

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

            _logger.LogDebug("Пользователь {Email} успешно вошел в систему", json.Email);
            return new OkObjectResult(new TokenJson { AccessToken = accessToken, RefreshToken = refreshToken });
        }

        public async Task<IActionResult> UpdateAsync(RefreshTokenJson json)
        {
            _logger.LogDebug("Запрос обновления токена для refresh token: {RefreshToken}", json.RefreshToken);
            var user = await _context.Users.FirstOrDefaultAsync(r => r.RefreshToken == json.RefreshToken);

            if (user == null)
            {
                _logger.LogWarning("Невалидный refresh token: {RefreshToken}", json.RefreshToken);
                return new UnauthorizedObjectResult("Invalid refresh token");
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

            _logger.LogDebug("Токены для пользователя {Email} успешно обновлены", user.Email);
            return new OkObjectResult(new TokenJson { AccessToken = accessToken, RefreshToken = refreshToken });
        }



        private async Task PublishInRMQAsync(Role role, Users people)
        {
            if (role.Name == "Specialist")
            {
                try
                {
                    await _publishEndpoint.Publish(new UserRegisteredEvent
                    {
                        UserId = people.Id,
                        Email = people.Email,
                        Role = role.Name
                    });

                    _logger.LogInformation("Message published to RabbitMQ for user {UserId} with role {Role}", people.Id, role.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish message for user {UserId}", people.Id);
                    return;
                }
            }
        }
    }
}
