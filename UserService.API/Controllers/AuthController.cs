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
            _logger.LogInformation("������ ����������� ������������ � email: {Email}", json.Email);
            try
            {
                string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
                bool isValidEmail = Regex.IsMatch(json.Email, pattern);

                if (!isValidEmail)
                {
                    _logger.LogWarning("�������� ������ email: {Email}", json.Email);
                    return BadRequest("�������� �����");
                }

                if (json.Password.Length < 8)
                {
                    _logger.LogWarning("������ ������� �������� ��� ������������: {Email}", json.Email);
                    return BadRequest("������ ������ ��������� �� 8 ��������");
                }

                int roleId = json.IsSpecialist ? 1 : 2;
                var role = await _context.Roles.FindAsync(roleId);

                if (role == null)
                {
                    _logger.LogError("���� � ID {RoleId} �� �������", roleId);
                    return BadRequest("���� �� �������");
                }

                var findUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == json.Email || u.Username == json.UserName);

                if (findUser != null)
                {
                    _logger.LogWarning("������������ � email {Email} ��� ����������", json.Email);
                    return BadRequest("��� ����� ��� ����������������");
                }

                var password = HasherPassword.HashPassword(json.Password); // �������� ������
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
                _logger.LogInformation("������������ {Email} ������� ���������������", json.Email);
                return Ok("������������ ������� ���������������");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "������ ��� ����������� ������������: {Email}", json.Email);
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUserJson json)
        {
            _logger.LogInformation("������� ����� ������������ � email: {Email}", json.Email);
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(e => e.Email == json.Email);

                if (user == null)
                {
                    _logger.LogWarning("������� ����� � �������������� email: {Email}", json.Email);
                    return BadRequest("��� ����� �� ����������������");
                }

                if (!HasherPassword.VerifyPassword(json.Password, user.PasswordHash))
                {
                    _logger.LogWarning("�������� ������ ��� ������������ � email: {Email}", json.Email);
                    return BadRequest("������������ ������");
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

                // ��������� refresh token � ���� ��� �������� � ���� ������ ��� ������������
                user.RefreshToken = refreshToken;
                await _context.SaveChangesAsync();

                _logger.LogInformation("������������ {Email} ������� ����� � �������", json.Email);
                return Ok(new TokenJson { AccessToken = accessToken, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "������ ��� ����� ������������ � email: {Email}", json.Email);
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateToken(RefreshTokenJson json)
        {
            _logger.LogInformation("������ ���������� ������ ��� refresh token: {RefreshToken}", json.RefreshToken);
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(r => r.RefreshToken == json.RefreshToken);

                if (user == null)
                {
                    _logger.LogWarning("���������� refresh token: {RefreshToken}", json.RefreshToken);
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

                _logger.LogInformation("������ ��� ������������ {Email} ������� ���������", user.Email);
                return Ok(new TokenJson { AccessToken = accessToken, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "������ ��� ���������� ������ ��� refresh token: {RefreshToken}", json.RefreshToken);
                return BadRequest(ex.Message);
            }
        }
    }
}
