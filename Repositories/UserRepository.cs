using Dapper;
using System.Data;
using RbacApi.Models;
using RbacApi.Controllers;

namespace RbacApi.Repositories
{
    public class UserRepository
    {
        private readonly IDbConnection _dbConnection;

        public UserRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<GetUsersHelper>> GetUsersAsync()
        {
            var sql = @"
                        SELECT u.user_id, u.username, u.email, r.role_name AS Role
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
            return await _dbConnection.ExecuteScalarAsync<int>(sql, user);
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

        public async Task AssignRoleToUserAsync(int userId, string roleName)
        {
            var sqlFetchRoleId = "SELECT role_id FROM Roles WHERE role_name = @RoleName";
            var roleId = await _dbConnection.ExecuteScalarAsync<int?>(sqlFetchRoleId, new { RoleName = roleName });

            if (roleId == null)
            {
                throw new Exception($"Role '{roleName}' not found.");
            }

            var sqlInsertUserRole = "INSERT INTO UserRoles (user_id, role_id) VALUES (@UserId, @RoleId)";
            await _dbConnection.ExecuteAsync(sqlInsertUserRole, new { UserId = userId, RoleId = roleId });
        }

        public async Task<string> GetUserRoleAsync(int userId)
        {
            const string sql = @"
                                SELECT r.role_name
                                FROM Users u
                                JOIN UserRoles ur ON u.user_id = ur.user_id
                                JOIN Roles r ON ur.role_id = r.role_id
                                WHERE u.user_id = @UserId";

            return await _dbConnection.QueryFirstOrDefaultAsync<string>(sql, new { UserId = userId });
        }

    }

    public class GetUsersHelper {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
}
