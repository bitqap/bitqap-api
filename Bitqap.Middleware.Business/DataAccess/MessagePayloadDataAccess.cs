using Bitqap.Middleware.Entity.SocketEntity;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Bitqap.Middleware.Business.DataAccess
{
    public class MessagePayloadDataAccess : IMessagePayloadDataAccess
    {
        readonly string _dbConnection;

        public MessagePayloadDataAccess(string dbConnection)
        {
            _dbConnection = dbConnection;
            SQLitePCL.Batteries.Init();
        }

        public async Task<MsgPayload> Create(MsgPayload entity)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"INSERT INTO MsgPayload (
                           MethodName,
                           RequestKey,
                           Payload,
                           Direction,
                           CreateDateTime,
                           UserId
                       )
                       VALUES (
                           @MethodName,
                           @RequestKey,
                           @Payload,
                           @Direction,
                           @CreateDateTime,
                           @UserId
                       ) returning id";
            var parameters = new
            {
                MethodName = entity.MethodName,
                RequestKey = entity.RequestKey,
                Payload = entity.Payload,
                Direction = entity.Direction.ToString(),
                CreateDateTime = DateTime.Now,
                UserId = entity.UserId
            };
            entity.Id = await connection.ExecuteScalarAsync<long>(query, parameters);
            return entity;
        }

        public async Task DeleteById(long id)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"DELETE FROM MsgPayload
                                            WHERE ID = @id";
            var parameters = new { id = id };
            await connection.ExecuteAsync(query, parameters);
        }

        public async Task<MsgPayload> FindById(long id)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"SELECT *
                          FROM MsgPayload where ID=@id";
            var parameters = new { id = id };
            return await connection.QueryFirstOrDefaultAsync<MsgPayload>(query, parameters);
        }

        public async Task<IEnumerable<MsgPayload>> FindByRequestKey(string ky)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"SELECT *
                          FROM MsgPayload where RequestKey=@RequestKey";
            var parameters = new { RequestKey = ky };
            return await connection.QueryAsync<MsgPayload>(query, parameters);
        }

        public async Task<MsgPayload> FindByRequestKeyAndDirection(string ky, MsgDirection direction)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"SELECT *
                          FROM MsgPayload where RequestKey=@RequestKey and Direction=@Direction";
            var parameters = new { RequestKey = ky, Direction = direction.ToString() };
            return await connection.QueryFirstOrDefaultAsync<MsgPayload>(query, parameters);
        }

        public async Task<IEnumerable<MsgPayload>> GelAll()
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"SELECT *
                          FROM MsgPayload";
            return await connection.QueryAsync<MsgPayload>(query);
        }

        public async Task<MsgPayload> Update(MsgPayload entity)
        {
            using var connection = new SqliteConnection(_dbConnection);
            var query = @"UPDATE MsgPayload
                       SET 
                           MethodName = @MethodName,
                           RequestKey = @RequestKey,
                           Payload = @Payload,
                           Direction = @Direction,
                     WHERE Id = @Id";
            var parameters = new { MethodName = entity.MethodName, RequestKey = entity.RequestKey, Payload = entity.Payload, Direction = entity.Direction.ToString(), id = entity.Id };
            await connection.ExecuteAsync(query, parameters);
            return entity;
        }
    }
}
