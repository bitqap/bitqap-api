using Bitqap.Middleware.Entity.SocketEntity;

namespace Bitqap.Middleware.Business.DataAccess
{
    public interface IMessagePayloadDataAccess : IBaseDataAccess<MsgPayload>
    {
        Task<IEnumerable<MsgPayload>> FindByRequestKey(string ky);
        Task<MsgPayload> FindByRequestKeyAndDirection(string ky, MsgDirection direction);
    }
}
