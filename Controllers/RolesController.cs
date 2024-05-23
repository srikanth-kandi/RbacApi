using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RbacApi.Repositories;
using RbacApi.Models;

namespace RbacApi.Controllers
{
    [Authorize] // Requires authentication
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RolesRepository _rolesRepository;

        public RolesController(RolesRepository roleRepository)
        {
            _rolesRepository = roleRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Role>> GetAllRoles()
        {
            return await _rolesRepository.GetAllRolesAsync();
        }

        // Assign role to a user
        [HttpPost("assign")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> AssignRole(RoleAssignment roleAssignment)
        {
            await _rolesRepository.AssignRoleToUserAsync(roleAssignment.UserId, roleAssignment.RoleName);
            return Ok(new { message = "Role assigned successfully" });
        }

        // Change user role
        [HttpPut("change")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> ChangeRole(RoleAssignment roleAssignment)
        {
            await _rolesRepository.ChangeUserRoleAsync(roleAssignment.UserId, roleAssignment.RoleName);
            return Ok(new { message = "Role changed successfully" });
        }

        // Remove role from a user
        [HttpDelete("remove/{userId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> RemoveRole(int userId)
        {
            await _rolesRepository.RemoveRoleFromUserAsync(userId);
            return Ok(new { message = "Role removed successfully" });
        }

        // Get roles for a user
        [HttpGet("{userId}")]
        [Authorize(Policy = "ViewerPolicy")]
        public async Task<ActionResult<IEnumerable<RoleAssignment>>> GetUserRoles(int userId)
        {
            var roles = await _rolesRepository.GetUserRolesAsync(userId);
            return Ok(roles);
        }
    }
}
