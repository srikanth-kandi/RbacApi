using Microsoft.AspNetCore.Authorization;

namespace RbacApi.Authorization
{
    public class CustomRoleRequirement : IAuthorizationRequirement
    {
        public string Role { get; }
        public string Permission { get; }

        public CustomRoleRequirement(string role, string permission)
        {
            Role = role;
            Permission = permission;
        }
    }
}
