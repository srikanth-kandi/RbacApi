using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RbacApi.Models;
using RbacApi.Repositories;

namespace RbacApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(UserRepository userRepository) : ControllerBase
    {
        private readonly UserRepository _userRepository = userRepository;

        // GET: api/Users
        [HttpGet]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult<IEnumerable<GetUsersHelper>>> GetUsers()
        {
            var users = await _userRepository.GetUsersAsync();
            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize(Policy = "ViewerPolicy")]
        public async Task<ActionResult<GetUsersHelper>> GetUser(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var role = await _userRepository.GetUserRoleAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found for the user" });
            }

            GetUsersHelper getUserHelper = new()
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = role
            };

            return Ok(getUserHelper);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        [Authorize(Policy = "EditorPolicy")]
        public async Task<IActionResult> PutUser(int id, [FromBody] UserUpdateHelper userUpdateHelper)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID has to be greater than 0" });
            }

            // Check if the user with id exists
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            UserUpdate updatedUser = new()
            {
                UserId = id,
                UpdatedAt = DateTime.UtcNow
            };

            // Update fields only if they are provided in the request
            if (!string.IsNullOrWhiteSpace(userUpdateHelper.Username))
            {
                updatedUser.Username = userUpdateHelper.Username;
            }

            if (!string.IsNullOrWhiteSpace(userUpdateHelper.Email))
            {
                updatedUser.Email = userUpdateHelper.Email;
            }

            var rowsAffected = await _userRepository.UpdateUserAsync(updatedUser);
            if (rowsAffected > 0)
            {
                return Ok(new { message = "User updated successfully" });
            }
            else
            {
                return StatusCode(500, new { message = "Error updating user" });
            }
        }

        // POST: api/Users
        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult<GetUsersHelper>> PostUser(UserRegister ur)
        {
            User user = new()
            {
                Username = ur.Username,
                Email = ur.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(ur.Password),
                Role = "viewer" // default role
            };
            var userId = await _userRepository.CreateUserAsync(user);
            user.UserId = userId;

            GetUsersHelper getUsersHelper = new()
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = "viewer"
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, getUsersHelper);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userRepository.DeleteUserAsync(id);
            if (deleted == 0)
            {
                return NotFound();
            }
            return NoContent();
        }
    }

    public class UserUpdate
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserUpdateHelper
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
    }

}
