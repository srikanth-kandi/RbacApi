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
        private readonly PermissionRepository _permissionRepository;

        public PermissionsController(PermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Permission>> GetAllPermissions()
        {
            return await _permissionRepository.GetAllPermissionsAsync();
        }

        // Implement other CRUD endpoints for permissions
    }
}
