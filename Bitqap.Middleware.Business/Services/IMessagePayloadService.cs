using Bitqap.Middleware.Entity.SocketEntity;

namespace Bitqap.Middleware.Business.Services
{
    public interface IMessagePayloadService
    {
        Task<MsgPayload> AddMessagePayload(MsgPayload msgPayload);
        Task<MsgPayload> GetPayloadByRequestIdAndDirection(string reqId, MsgDirection direction);
    }
}
