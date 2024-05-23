using Microsoft.AspNetCore.Authorization;

namespace RbacApi.Authorization
{
    public class CustomRoleRequirement : IAuthorizationRequirement
    {
        public string Role { get; }

        public CustomRoleRequirement(string role)
        {
            Role = role;
        }
    }
}
