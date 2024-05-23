namespace RbacApi.Models
{
    public class RoleAssignment
    {
        public int UserId { get; set; }
        public required string RoleName { get; set; }
    }
}