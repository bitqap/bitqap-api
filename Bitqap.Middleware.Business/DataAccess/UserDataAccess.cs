using Bitqap.Middleware.Entity.ApiEntity;
using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            entity.Id = await connection.ExecuteScalarAsync<long>("INSERT INTO User (Username, Firstname,Lastname,Password,RegisterDate)" +
              "VALUES (@Username, @Firstname,@Lastname,@Password,@RegisterDate) returning id", entity);
            return entity;
        }

        public async Task DeleteById(long id)
        {
            throw new NotImplementedException();
        }

        public async Task<User> FindById(long id)
        {
            throw new NotImplementedException();
        }

        public Task<User> FindByUsername(string username)
        {
            throw new NotImplementedException();
        }

        public async Task<IQueryable<User>> GelAll()
        {
            throw new NotImplementedException();
        }

        public async Task<User> Update(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
