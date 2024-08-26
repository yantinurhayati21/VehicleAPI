using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleAPI.Dtos;
using VehicleAPI.Helpers;
using VehicleAPI.Models;
using VehicleAPI.Repository;

namespace VehicleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IUserRepository userRepository, JwtTokenService jwtTokenService, ILogger<AuthenticationController> logger)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userExists = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (userExists != null)
            {
                return Conflict(new { message = "Email is already in use." });
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsAdmin = dto.IsAdmin,
            };

            try
            {
                var createdUser = await _userRepository.AddUserAsync(user);
                return Created("Success", createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return Unauthorized(new { message = "Invalid Credentials" });
            }

            var jwt = _jwtTokenService.Generate(user.Id, user.IsAdmin);

            Response.Cookies.Append("jwt", jwt, new CookieOptions
            {
                HttpOnly = true,
            });

            return Ok(new
            {
                jwt,
                message = "You have successfully logged in"
            });
        }

        [HttpGet("user")]
        public async Task<IActionResult> User()
        {
            try
            {
                var jwt = Request.Cookies["jwt"];
                if (jwt == null)
                {
                    return Unauthorized(new { message = "No JWT token provided" });
                }

                var token = _jwtTokenService.Verify(jwt);
                int userId = int.Parse(token.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
                var user = await _userRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user data");
                return Unauthorized();
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            _logger.LogInformation("User logged out");
            return Ok(new { message = "Successfully logged out" });
        }
    }
}
