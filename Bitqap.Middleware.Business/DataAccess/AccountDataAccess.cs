using Bitqap.Middleware.Entity.BusinessEntity;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bitqap.Middleware.Business.DataAccess
{
    public class AccountDataAccess : IAccountDataAccess
    {
        readonly string _dbConnection;

        public AccountDataAccess(string dbConnection)
        {
            _dbConnection = dbConnection;
            SQLitePCL.Batteries.Init();
        }
        public async Task<Account> Create(Account entity)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"INSERT INTO Account (
                        
                        UserId,
                        AccountKey,
                        AccountName,
                        CreateDate
                    )
                    VALUES (
                        
                        @UserId,
                        @AccountKey,
                        @AccountName,
                        @CreateDate
                    )
                        returning id";
            entity.CreateDate = DateTime.Now;
            entity.Id = await connection.ExecuteScalarAsync<long>(query, entity);
            return entity;
        }

        public async Task DeleteById(long id)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"DELETE FROM Account
                                            WHERE ID = @id";
            var parameters = new { id = id };
            await connection.ExecuteAsync(query, parameters);
        }

        public async Task<Account> FindByAccountKey(string accountKey)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"SELECT ID,
                        UserId,
                        AccountKey,
                        AccountName,
                        CreateDate
                          FROM Account where AccountKey=@key";
            var parameters = new { key = accountKey };
            return await connection.QueryFirstOrDefaultAsync<Account>(query, parameters);
        }

        public async Task<Account> FindById(long id)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"SELECT ID,
                        UserId,
                        AccountKey,
                        AccountName,
                        CreateDate
                          FROM Account where ID=@id";
            var parameters = new { id = id };
            return await connection.QueryFirstOrDefaultAsync<Account>(query, parameters);
        }

        public async Task<Account> FindByUserId(string userId)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"SELECT ID,
                        UserId,
                        AccountKey,
                        AccountName,
                        CreateDate
                          FROM Account where UserId=@id";
            var parameters = new { id = userId };
            return await connection.QueryFirstOrDefaultAsync<Account>(query, parameters);
        }

        public async Task<IEnumerable<Account>> GelAll()
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"SELECT ID,
                        UserId,
                        AccountKey,
                        AccountName,
                        CreateDate
                          FROM Account";

            return await connection.QueryAsync<Account>(query);
        }

        public async Task<Account> Update(Account entity)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"UPDATE Account
                           SET 
                               UserId = @UserId,
                               AccountKey = @AccountKey,
                               AccountName = @AccountName
                         WHERE ID = @Id";
            await connection.ExecuteAsync(query, entity);
            return entity;
        }
    }
}
