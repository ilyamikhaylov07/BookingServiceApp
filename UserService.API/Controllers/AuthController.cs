using Microsoft.AspNetCore.Mvc;
using UserService.API.DTOs;
using UserService.API.Services;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("UserService/[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger, AuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationJson json)
        {
            var result = await _authService.RegisterAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginUserJson json)
        {
            var result = await _authService.SignInAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateToken(RefreshTokenJson json)
        {
            var result = await _authService.UpdateAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");

            return Ok(result);
        }
    }
}
