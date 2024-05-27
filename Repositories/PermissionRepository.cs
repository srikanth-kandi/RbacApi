using Dapper;
using System.Data;
using RbacApi.Models;

namespace RbacApi.Repositories
{
    public interface IPermissionsRepository
    {
        Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
        Task<bool> GrantPermissionAsync(int userId, string permissionName);
        Task<bool> RevokePermissionAsync(int userId, string permissionName);
    }

    public class PermissionRepository(IDbConnection dbConnection) : IPermissionsRepository
    {
        private readonly IDbConnection _dbConnection = dbConnection;

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            var sql = "SELECT * FROM Permissions";
            return await _dbConnection.QueryAsync<Permission>(sql);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
        {
            var sql = @"SELECT p.permission_name 
                        FROM Permissions p
                        JOIN UserPermissions up ON p.permission_id = up.permission_id
                        WHERE up.user_id = @UserId";

            return await _dbConnection.QueryAsync<string>(sql, new { UserId = userId });
        }

        public async Task<bool> GrantPermissionAsync(int userId, string permissionName)
        {
            var sql = @"INSERT INTO UserPermissions (user_id, permission_id)
                        SELECT @UserId, permission_id FROM Permissions WHERE permission_name = @PermissionName";

            var result = await _dbConnection.ExecuteAsync(sql, new { UserId = userId, PermissionName = permissionName });
            return result > 0;
        }

        public async Task<bool> RevokePermissionAsync(int userId, string permissionName)
        {
            var sql = @"DELETE FROM UserPermissions 
                        WHERE user_id = @UserId 
                        AND permission_id = (SELECT permission_id FROM Permissions WHERE permission_name = @PermissionName)";

            var result = await _dbConnection.ExecuteAsync(sql, new { UserId = userId, PermissionName = permissionName });
            return result > 0;
        }
    }
}
