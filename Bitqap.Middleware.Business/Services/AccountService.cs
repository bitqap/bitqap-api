using Bitqap.Middleware.Business.Clients;
using Bitqap.Middleware.Business.DataAccess;
using Bitqap.Middleware.Business.Extensions;
using Bitqap.Middleware.Business.Utils;
using Bitqap.Middleware.Entity.ApiEntity;
using Bitqap.Middleware.Entity.BusinessEntity;
using Bitqap.Middleware.Entity.SocketEntity;
using Newtonsoft.Json;
using NLog;

namespace Bitqap.Middleware.Business.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountDataAccess _accountDataAccess;
        private readonly IMappingExtension _mappingExtension;
        private readonly IUserService _userservice;
        private readonly IMessagePayloadService _msgPayloadService;
        private readonly SocketClient _socketClient;
        private readonly ApiSettings _apiSettings;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public AccountService(IAccountDataAccess accountDataAccess, SocketClient socketClient, IUserService userService, IMessagePayloadService msgPayloadService, IMappingExtension mappingExtension, ApiSettings apiSettings)
        {
            _accountDataAccess = accountDataAccess;
            _socketClient = socketClient;
            _userservice = userService;
            _msgPayloadService = msgPayloadService;
            _mappingExtension = mappingExtension;
            _apiSettings = apiSettings;
        }

        public async Task<Account> CreateAccount(Account entity)
        {
            _logger.Log(NLog.LogLevel.Debug, "Service called for create new Account", default(Exception));
            var user = await _userservice.GetUserById(entity.UserId);
            if(user == null) throw new BitqapBusinessException("Requested user not found", "USER_NOT_FOUND");
            var existedAccount = await _accountDataAccess.FindByAccountKey(entity.AccountKey);
            if (existedAccount != null) throw new BitqapBusinessException("Account already exists", "ACCOUNT_EXISTS");
            var result = await _accountDataAccess.Create(entity);

            return result;
        }

        public Task DeleteAccountById(long id)
        {
            throw new NotImplementedException();
        }

        public async Task<BasicResponse> RequestAccountBalanceByAccountKey(string accountKey, long userId)
        {
            _logger.Log(NLog.LogLevel.Debug, "Service called FindAccountBalanceByAccountKey", default(Exception));
            var user = await _userservice.GetUserById(userId);
            if (user == null) throw new BitqapBusinessException("Requested user not found", "USER_NOT_FOUND");
            var existedAccount = await _accountDataAccess.FindByAccountKey(accountKey);
            if (existedAccount == null) throw new BitqapBusinessException("Requested account not found", "ACCOUNT_NOT_FOUND");
            var ky = Guid.NewGuid().ToString("N");
            var command = "checkbalance";
            GetBalanceRequest socketRequest = new GetBalanceRequest { Commant = command, MessageType = "direct", AccountKey = accountKey, RequestId = ky };

            var pylod = Newtonsoft.Json.JsonConvert.SerializeObject(socketRequest);
            var msgPyld = new MsgPayload { Direction = MsgDirection.SENT, MethodName = command, Payload = pylod, RequestKey = ky, UserId = userId };
            await _socketClient.SendMessage(msgPyld);

            return new BasicResponse { Command=command, RequestId=ky, Message = "Request accepted" };
        }

        public async Task<Object> GetRequestedBalance(string key, long userId)
        {
            _logger.Log(NLog.LogLevel.Debug, "Service called GetRequestedBalance", default(Exception));
            var user = await _userservice.GetUserById(userId);
            if (user == null) throw new BitqapBusinessException("Requested user not found", "USER_NOT_FOUND");
            var loggedSocketRequest = await _msgPayloadService.GetPayloadByRequestIdAndDirection(key, MsgDirection.SENT);
            if (loggedSocketRequest == null) throw new BitqapBusinessException($"Socket request not found by id: {key}", "SOCKET_REQUEST_NOT_FOUND");
            if (loggedSocketRequest.UserId != userId) throw new BitqapBusinessException("Socket request not found by user", "SOCKET_REQUEST_NOT_FOUND");

            var loggedSocketResponse = await _msgPayloadService.GetPayloadByRequestIdAndDirection(key, MsgDirection.RECEIVED);
            if (loggedSocketResponse == null) throw new BitqapBusinessException($"Socket response not found by id: {key}", "SOCKET_RESPONSE_NOT_FOUND");

            return JsonConvert.DeserializeObject<object>(loggedSocketResponse.Payload);
        }

        public Task<Account> FindAccountByAccountKey(string accountKey)
        {
            throw new NotImplementedException();
        }

        public Task<Account> FindAccountById(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Account> FindAccountByUserId(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Account>> GelAllAccounts()
        {
            throw new NotImplementedException();
        }

        public Task<Account> UpdateAccount(Account entity)
        {
            throw new NotImplementedException();
        }

        public async Task<BasicResponse> RequestTransactionsByAccountKey(string accountKey, long userId)
        {
            _logger.Log(NLog.LogLevel.Debug, "Service called RequestTransactionsByAccountKey", default(Exception));
            var user = await _userservice.GetUserById(userId);
            if (user == null) throw new BitqapBusinessException("Requested user not found", "USER_NOT_FOUND");
            var existedAccount = await _accountDataAccess.FindByAccountKey(accountKey);
            if (existedAccount == null) throw new BitqapBusinessException("Requested account not found", "ACCOUNT_NOT_FOUND");
            var ky = Guid.NewGuid().ToString("N");
            var command = "getHistory";
            GetTransactionsRequest socketRequest = new GetTransactionsRequest { Commant = command, MessageType = "direct", AccountKey = accountKey, RequestId = ky };

            var pylod = JsonConvert.SerializeObject(socketRequest);
            var msgPyld = new MsgPayload { Direction = MsgDirection.SENT, MethodName = command, Payload = pylod, RequestKey = ky, UserId = userId };
            await _socketClient.SendMessage(msgPyld);

            return new BasicResponse { Command = command, RequestId = ky, Message = "Request accepted" };
        }

        public async Task<object> GetRequestedTransactions(string key, long userId)
        {
            _logger.Log(NLog.LogLevel.Debug, "Service called GetRequestedTransactions", default(Exception));
            var user = await _userservice.GetUserById(userId);
            if (user == null) throw new BitqapBusinessException("Requested user not found", "USER_NOT_FOUND");
            var loggedSocketRequest = await _msgPayloadService.GetPayloadByRequestIdAndDirection(key, MsgDirection.SENT);
            if (loggedSocketRequest == null) throw new BitqapBusinessException($"Socket request not found by id: {key}", "SOCKET_REQUEST_NOT_FOUND");
            if (loggedSocketRequest.UserId != userId) throw new BitqapBusinessException("Socket request not found by user", "SOCKET_REQUEST_NOT_FOUND");

            var loggedSocketResponse = await _msgPayloadService.GetPayloadByRequestIdAndDirection(key, MsgDirection.RECEIVED);
            if (loggedSocketResponse == null) throw new BitqapBusinessException($"Socket response not found by id: {key}", "SOCKET_RESPONSE_NOT_FOUND");

            return JsonConvert.DeserializeObject<object>(loggedSocketResponse.Payload);
        }
    }
}
