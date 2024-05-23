namespace RbacApi.Models
{
    public class Permission
    {
        public int PermissionId { get; set; }
        public required string PermissionName { get; set; }
        public string? Description { get; set; }
    }
}
