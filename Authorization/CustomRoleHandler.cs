using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RbacApi.Authorization
{
    public class CustomRoleHandler : AuthorizationHandler<CustomRoleRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CustomRoleHandler> _logger;

        public CustomRoleHandler(IHttpContextAccessor httpContextAccessor, ILogger<CustomRoleHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomRoleRequirement requirement)
        {
            var user = _httpContextAccessor.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                return Task.CompletedTask;
            }

            var userRoles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            if (userRoles.Contains(requirement.Role) || 
                (requirement.Role == "editor" && userRoles.Contains("admin")) ||
                (requirement.Role == "viewer" && (userRoles.Contains("admin") || userRoles.Contains("editor"))))
            {
                context.Succeed(requirement);
            }
            else
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                _httpContextAccessor.HttpContext.Response.ContentType = "application/json";
                var responseMessage = new { message = $"Requires {requirement.Role} role to access this API" };
                _httpContextAccessor.HttpContext.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(responseMessage)).Wait();
            }

            return Task.CompletedTask;
        }
    }
}
