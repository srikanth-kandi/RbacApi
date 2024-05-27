using Dapper;
using System.Data;
using RbacApi.Models;
using RbacApi.Controllers;

namespace RbacApi.Repositories
{
    public class UserRepository(IDbConnection dbConnection, IPermissionsRepository permissionsRepository)
    {
        private readonly IDbConnection _dbConnection = dbConnection;
        private readonly IPermissionsRepository _permissionsRepository = permissionsRepository;

        public async Task<IEnumerable<GetUsersHelper>> GetUsersAsync()
        {
            var sql = @"SELECT u.user_id, u.username, u.email, r.role_name AS Role
                        FROM Users u
                        LEFT JOIN UserRoles ur ON u.user_id = ur.user_id
                        LEFT JOIN Roles r ON ur.role_id = r.role_id";
            return await _dbConnection.QueryAsync<GetUsersHelper>(sql);
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var sql = "SELECT * FROM Users WHERE user_id = @UserId";
            return await _dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
        }

        public async Task<int> CreateUserAsync(User user)
        {
            var sql = @"INSERT INTO Users (username, password_hash, email, created_at, updated_at)
                        VALUES (@Username, @PasswordHash, @Email, @CreatedAt, @UpdatedAt)
                        RETURNING user_id";
            var userId = await _dbConnection.ExecuteScalarAsync<int>(sql, user);
        
            // Assign default role
            await AssignRoleToUserAsync(userId, "viewer");
            // Assign default permission
            await _permissionsRepository.GrantPermissionAsync(userId, "view_content");
            
            return userId;
        }

        public async Task<int> UpdateUserAsync(UserUpdate user)
        {
            var sql = @"UPDATE Users SET
                        username = COALESCE(@Username, username),
                        email = COALESCE(@Email, email),
                        updated_at = @UpdatedAt
                        WHERE user_id = @UserId";
            return await _dbConnection.ExecuteAsync(sql, user);
        }

        public async Task<int> DeleteUserAsync(int userId)
        {
            var sql = "DELETE FROM Users WHERE user_id = @UserId";
            return await _dbConnection.ExecuteAsync(sql, new { UserId = userId });
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var sql = "SELECT * FROM Users WHERE username = @Username";
            return await _dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
        {
            var sql = @"SELECT p.permission_name
                    FROM Permissions p
                    JOIN UserPermissions up ON p.permission_id = up.permission_id
                    WHERE up.user_id = @UserId";
            return await _dbConnection.QueryAsync<string>(sql, new { UserId = userId });
        }

        public async Task<string> GetUserRoleAsync(int userId)
        {
            var sql = @"SELECT r.role_name
                    FROM Roles r
                    JOIN UserRoles ur ON r.role_id = ur.role_id
                    WHERE ur.user_id = @UserId";
            return await _dbConnection.QuerySingleOrDefaultAsync<string>(sql, new { UserId = userId });
        }

        public async Task AssignRoleToUserAsync(int userId, string roleName)
        {
            var sql = @"INSERT INTO UserRoles (user_id, role_id)
                    SELECT @UserId, role_id FROM Roles WHERE role_name = @RoleName";
            await _dbConnection.ExecuteAsync(sql, new { UserId = userId, RoleName = roleName });
        }

    }

    public class GetUsersHelper
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
}
