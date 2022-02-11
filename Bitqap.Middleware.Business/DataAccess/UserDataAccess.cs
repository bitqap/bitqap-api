using Bitqap.Middleware.Entity.BusinessEntity;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bitqap.Middleware.Business.DataAccess
{
    public class UserDataAccess : IUserDataAccess
    {
        readonly string _dbConnection;

        public UserDataAccess(string dbConnection)
        {
            _dbConnection = dbConnection;
            SQLitePCL.Batteries.Init();
        }

        public async Task<User> Create(User entity)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"INSERT INTO User(Username, Firstname, Lastname, Password, RegisterDate)" +
              "VALUES (@Username, @Firstname,@Lastname,@Password,@RegisterDate) returning id";
            entity.RegisterDate = DateTime.Now;
            entity.Id = await connection.ExecuteScalarAsync<long>(query, entity);
            return entity;
        }

        public async Task DeleteById(long id)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"DELETE FROM User
                                            WHERE ID = @id";
            var parameters = new { id = id };
            await connection.ExecuteAsync(query, parameters);
        }

        public async Task<User> FindById(long id)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"SELECT ID,
                               Username,
                               Firstname,
                               Lastname,
                               Password,
                               RegisterDate
                          FROM User where ID=@id";
            var parameters = new { id = id };
            return await connection.QueryFirstOrDefaultAsync<User>(query, parameters);
        }

        public async Task<User> FindByUsername(string username)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"SELECT ID,
                               Username,
                               Firstname,
                               Lastname,
                               Password,
                               RegisterDate
                          FROM User where Username=@username";
            var parameters = new { username = username};
            return await connection.QueryFirstOrDefaultAsync<User>(query, parameters);
        }

        public async Task<IEnumerable<User>> GelAll()
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"SELECT ID,
                               Username,
                               Firstname,
                               Lastname,
                               Password,
                               RegisterDate
                          FROM User";

            return await connection.QueryAsync<User>(query);
        }

        public async Task<User> Update(User entity)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"UPDATE User
                           SET
                               Username = @username,
                               Firstname = @firstname,
                               Lastname = @lastname,
                               Password = @password
                         WHERE ID = @id";
            var parameters = new { username = entity.Username, firstname = entity.Firstname, lastname = entity.Lastname, password = entity.Password, id=entity.Id };
            await connection.ExecuteAsync(query,parameters);
            return entity;
        }
    }
}
