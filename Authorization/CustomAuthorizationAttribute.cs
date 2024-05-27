using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RbacApi.Repositories;

namespace RbacApi.Authorization
{
    public class CustomAuthorizationAttribute(string role, string permission) : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _role = role;
        private readonly string _permission = permission;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity!.IsAuthenticated)
            {
                context.Result = new JsonResult(new { message = "Authentication required to access this API" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var userRepository = context.HttpContext.RequestServices.GetService<UserRepository>();
            var userId = int.Parse(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var userRole = await userRepository!.GetUserRoleAsync(userId);
            var userPermissions = await userRepository.GetUserPermissionsAsync(userId);

            if (userRole != _role && !userPermissions.Contains(_permission))
            {
                context.Result = new JsonResult(new { message = $"Permission '{_permission}' or role '{_role}' required to access this API" })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}