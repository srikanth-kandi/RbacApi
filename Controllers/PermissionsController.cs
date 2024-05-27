using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RbacApi.Repositories;
using RbacApi.Models;

namespace RbacApi.Controllers
{
    [Authorize] // Requires authentication
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionsRepository _permissionRepository;

        public PermissionsController(IPermissionsRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Permission>> GetAllPermissions()
        {
            return await _permissionRepository.GetAllPermissionsAsync();
        }

        [HttpGet("{userId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserPermissions(int userId)
        {
            var permissions = await _permissionRepository.GetUserPermissionsAsync(userId);
            if (permissions == null)
            {
                return NotFound();
            }
            return Ok(permissions);
        }

        [HttpPost("{userId}/grant")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> GrantPermission(int userId, [FromBody] string permissionName)
        {
            var result = await _permissionRepository.GrantPermissionAsync(userId, permissionName);
            if (!result)
            {
                return BadRequest(new { message = "Failed to grant permission" });
            }
            return Ok(new { message = "Permission granted successfully" });
        }

        [HttpPost("{userId}/revoke")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> RevokePermission(int userId, [FromBody] string permissionName)
        {
            var result = await _permissionRepository.RevokePermissionAsync(userId, permissionName);
            if (!result)
            {
                return BadRequest(new { message = "Failed to revoke permission" });
            }
            return Ok(new { message = "Permission revoked successfully" });
        }
    }
}
