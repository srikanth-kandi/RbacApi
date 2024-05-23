using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RbacApi.Models;
using RbacApi.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RbacApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthController(UserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegister user)
        {
            User userModel = new()
            {
                Username = user.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Email = user.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Role = "viewer" // default role
            };

            var userId = await _userRepository.CreateUserAsync(userModel);
            userModel.UserId = userId;

            await _userRepository.AssignRoleToUserAsync(userId, "viewer");

            GetUsersHelper getUserHelper = new()
            {
                UserId = userModel.UserId,
                Username = userModel.Username,
                Email = userModel.Email,
                Role = "viewer"
            };

            return CreatedAtAction(nameof(Register), new { id = userModel.UserId }, getUserHelper);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin model)
        {
            var user = await _userRepository.GetUserByUsernameAsync(model.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            var role = await _userRepository.GetUserRoleAsync(user.UserId);
            user.Role = role;

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }


        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                // Include standard claims (e.g., username)
                new(JwtRegisteredClaimNames.Sub, user.Username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"] ?? "YourSecretKey"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

    public class UserRegister
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
    }

    public class UserLogin
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
