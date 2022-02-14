using Bitqap.Middleware.Business.DataAccess;
using Bitqap.Middleware.Entity.SocketEntity;
using NLog;

namespace Bitqap.Middleware.Business.Services
{
    public class MessagePayloadService : IMessagePayloadService
    {
        readonly IMessagePayloadDataAccess _msgPayloadDataAccess;
        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public MessagePayloadService(IMessagePayloadDataAccess msgPayloadDataAccess)
        {
            _msgPayloadDataAccess = msgPayloadDataAccess;
        }

        public async Task<MsgPayload> AddMessagePayload(MsgPayload msgPayload)
        {
            _logger.Log(NLog.LogLevel.Debug, "Service called AddMessagePayload", default(Exception));
            return await _msgPayloadDataAccess.Create(msgPayload);
        }

        public async Task<MsgPayload> GetPayloadByRequestIdAndDirection(string reqId, MsgDirection direction)
        {
            _logger.Log(NLog.LogLevel.Debug, "Service called GetPayloadByRequestIdAndDirection", default(Exception));
            return await _msgPayloadDataAccess.FindByRequestKeyAndDirection(reqId, direction);
        }
    }
}
