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
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var role = await _userRepository.GetUserRoleAsync(user.UserId);
            user.Role = role;

            var permissions = await _userRepository.GetUserPermissionsAsync(user.UserId);

            var token = GenerateJwtToken(user, permissions.ToList());
            return Ok(new { token });
        }


        private string GenerateJwtToken(User user, List<string> permissions)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Role, user.Role)
            };

            // Add permissions as claims
            claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
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
