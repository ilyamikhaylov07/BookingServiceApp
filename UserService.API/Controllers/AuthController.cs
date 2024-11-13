using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Json;
using System.IdentityModel.Tokens.Jwt;
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
        private readonly UserDbContext context;
        public AuthController(UserDbContext context)
        {
            this.context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationJson json)
        {
            try
            {
                string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
                bool isValidEmail = Regex.IsMatch(json.Email, pattern);
                if (!isValidEmail)
                {
                    return BadRequest("�������� �����");
                }
                if (json.Password.Length < 8)
                {
                    return BadRequest("������ ������ ��������� �� 8 ��������");
                }
                int roleId = json.IsSpecialist ? 1 : 2;
                var role = await context.Roles.FindAsync(roleId);
                var password = HasherPassword.HashPassword(json.Password); // �������� ������

                if (role == null)
                {
                    return BadRequest("���� �� �������");
                }

                var findUser = await context.Users.FirstOrDefaultAsync(u => u.Email == json.Email);
                var people = new Users()
                {
                    Username = json.UserName,
                    Email = json.Email,
                    PasswordHash = password,
                    Role = role,
                    Created = DateTime.UtcNow
                };
                context.Users.Add(people);
                await context.SaveChangesAsync();
                return Ok("������������ ������� ���������������");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }


        }

        [HttpPost]
        public async Task<IActionResult> LoginUser(LoginUserJson json)
        {
            try
            {
                var user = context.Users.FirstOrDefaultAsync(e => e.Email == json.Email);
                if (user.Result == null)
                {
                    return BadRequest("��� ����� �� ����������������");
                }
                if (!HasherPassword.VerifyPassword(json.Password, user.Result.PasswordHash))
                {
                    return BadRequest("������������ ������");
                }
                if(user.Result.RoleId == 1)
                {
                    return BadRequest("�� �������� ��� ����������, �� �� �� ��������� �������");
                } 
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Result.Email),
                    new Claim(ClaimTypes.Role, "User")
                };

                // ������� access token
                var accessToken = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(2),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymSecurityKey(), SecurityAlgorithms.HmacSha256)
                );

                string accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

                // ������� refresh token � ����� ���������� ������ ��������
                var refreshToken = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(7), // ���������� ���� �������� ��� refresh token
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymSecurityKey(), SecurityAlgorithms.HmacSha256)
                );

                string refreshTokenString = new JwtSecurityTokenHandler().WriteToken(refreshToken);

                // ��������� refresh token � ���� ��� �������� � ���� ������ ��� ������������
                user.Result.RefreshToken = refreshTokenString;
                await context.SaveChangesAsync();

                // ���������� ��� ������ �������
                return Ok(new TokenJson { AccessToken = accessTokenString, RefreshToken = refreshTokenString });


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
        [HttpPost]
        public async Task<IActionResult> LoginSpecialist(LoginUserJson json)
        {
            try
            {
                var user = context.Users.FirstOrDefaultAsync(e => e.Email == json.Email);
                if (user.Result == null)
                {
                    return BadRequest("��� ����� �� ����������������");
                }
                if (!HasherPassword.VerifyPassword(json.Password, user.Result.PasswordHash))
                {
                    return BadRequest("������������ ������");
                }
                if (user.Result.RoleId == 2)
                {
                    return BadRequest("�� �������� ��� ������, ������� �������");
                }
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Result.Email),
                    new Claim(ClaimTypes.Role, "Specialist")
                };

                // ������� access token
                var accessToken = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(2),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymSecurityKey(), SecurityAlgorithms.HmacSha256)
                );

                string accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

                // ������� refresh token � ����� ���������� ������ ��������
                var refreshToken = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(7), // ���������� ���� �������� ��� refresh token
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymSecurityKey(), SecurityAlgorithms.HmacSha256)
                );

                string refreshTokenString = new JwtSecurityTokenHandler().WriteToken(refreshToken);

                // ��������� refresh token � ���� ��� �������� � ���� ������ ��� ������������
                user.Result.RefreshToken = refreshTokenString;
                await context.SaveChangesAsync();

                // ���������� ��� ������ �������
                return Ok(new TokenJson { AccessToken = accessTokenString, RefreshToken = refreshTokenString });


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

    }
}
