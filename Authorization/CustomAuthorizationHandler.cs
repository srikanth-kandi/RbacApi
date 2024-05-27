using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RbacApi.Authorization
{
    public class CustomAuthorizationHandler : AuthorizationHandler<CustomAuthorizationRequirement>
    {
        private readonly ILogger<CustomAuthorizationHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomAuthorizationHandler(ILogger<CustomAuthorizationHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomAuthorizationRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var requestMethod = httpContext!.Request.Method;

            var requiredPermission = GetRequiredPermission(requestMethod);

            if (!context.User.Identity!.IsAuthenticated)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // Get user permissions
            var userPermissions = context.User.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToList();

            _logger.LogInformation("Required Roles: {Roles}", requirement.RequiredRoles);
            _logger.LogInformation("Required Permissions: {Permissions}", requirement.RequiredPermissions);

            if (userPermissions.Contains(requiredPermission))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }

        private static string GetRequiredPermission(string requestMethod)
        {
            // Map request methods to required permissions
            switch (requestMethod)
            {
                case "GET":
                    return "view_content";
                case "POST":
                    return "create_content";
                case "PUT":
                    return "edit_content";
                case "DELETE":
                    return "delete_content";
                default:
                    return null!; // Invalid request method
            }
        }
    }
}