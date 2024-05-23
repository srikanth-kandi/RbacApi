using Dapper;
using System.Data;
using RbacApi.Models;

namespace RbacApi.Repositories
{
    public class PermissionRepository
    {
        private readonly IDbConnection _dbConnection;

        public PermissionRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            var sql = "SELECT * FROM Permissions";
            return await _dbConnection.QueryAsync<Permission>(sql);
        }

        // Other CRUD operations for permissions
    }
}
