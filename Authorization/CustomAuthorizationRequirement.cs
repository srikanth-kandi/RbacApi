using Microsoft.AspNetCore.Authorization;

namespace RbacApi.Authorization
{
    public class CustomAuthorizationRequirement : IAuthorizationRequirement
    {
        public List<string> RequiredRoles { get; }
        public List<string> RequiredPermissions { get; }

        public CustomAuthorizationRequirement(List<string> requiredRoles, List<string> requiredPermissions)
        {
            RequiredRoles = requiredRoles ?? [];
            RequiredPermissions = requiredPermissions ?? [];
        }
    }
}