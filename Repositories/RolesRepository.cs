using Dapper;
using System.Data;
using RbacApi.Models;

namespace RbacApi.Repositories
{
    public class RolesRepository
    {
        private readonly IDbConnection _dbConnection;

        public RolesRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            var sql = "SELECT * FROM Roles";
            return await _dbConnection.QueryAsync<Role>(sql);
        }

        public async Task AssignRoleToUserAsync(int userId, string roleName)
        {
            var sql = @"INSERT INTO UserRoles (user_id, role_id)
                        SELECT @UserId, role_id FROM Roles WHERE role_name = @RoleName
                        ON CONFLICT (user_id, role_id) DO NOTHING";
            await _dbConnection.ExecuteAsync(sql, new { UserId = userId, RoleName = roleName });
        }

        public async Task ChangeUserRoleAsync(int userId, string newRoleName)
        {
            var sql = @"DELETE FROM UserRoles WHERE user_id = @UserId;
                        INSERT INTO UserRoles (user_id, role_id)
                        SELECT @UserId, role_id FROM Roles WHERE role_name = @NewRoleName";
            await _dbConnection.ExecuteAsync(sql, new { UserId = userId, NewRoleName = newRoleName });
        }

        public async Task RemoveRoleFromUserAsync(int userId)
        {
            var sql = "DELETE FROM UserRoles WHERE user_id = @UserId";
            await _dbConnection.ExecuteAsync(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<RoleAssignment>> GetUserRolesAsync(int userId)
        {
            var sql = @"SELECT ur.user_id, r.role_name
                        FROM UserRoles ur
                        JOIN Roles r ON ur.role_id = r.role_id
                        WHERE ur.user_id = @UserId";
            return await _dbConnection.QueryAsync<RoleAssignment>(sql, new { UserId = userId });
        }
    }
}
