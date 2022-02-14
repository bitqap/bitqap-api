using Bitqap.Middleware.Business.DataAccess;
using Bitqap.Middleware.Business.Extensions;
using Bitqap.Middleware.Business.Utils;
using Bitqap.Middleware.Entity.ApiEntity;
using Bitqap.Middleware.Entity.BusinessEntity;
using NLog;

namespace Bitqap.Middleware.Business.Services
{
    public class UserService : IUserService
    {
        readonly IUserDataAccess _userDataAccess;
        readonly IMappingExtension _mappingExtension;
        readonly ApiSettings _apiSettings;
        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public UserService(IUserDataAccess userDataAccess, IMappingExtension mappingExtension, ApiSettings apiSettings)
        {
            _userDataAccess = userDataAccess;
            _mappingExtension = mappingExtension;
            _apiSettings = apiSettings;
        }

        [Obsolete]
        public async Task<User> RegisterNewUser(User entity)
        {
            _logger.Log(NLog.LogLevel.Debug, "Service called for create new User", default(Exception));
            var existedUser = await _userDataAccess.FindByUsername(entity.Username);
            if (existedUser != null) throw new BitqapBusinessException("User already exists", "USER_EXISTS");

            var result = await _userDataAccess.Create(entity);

            return result;
        }

        public async Task<User> LoginUser(LoginRequest entity)
        {
            _logger.Log(NLog.LogLevel.Debug, "Service called for login user", default(Exception));
            var existedUser = await _userDataAccess.FindByUsername(entity.UserName);
            if (existedUser == null) throw new BitqapBusinessException("Invalid username or password", "INVALID_LOGIN");
            if (!string.Equals(existedUser.Password, entity.Password, StringComparison.InvariantCulture)) throw new BitqapBusinessException("Invalid username or password", "INVALID_LOGIN");

            return existedUser;
        }

        public async Task<User> UpdateUser(User entity)
        {
            _logger.Log(NLog.LogLevel.Debug, "Service called for UpdateUser", default(Exception));
            var existedUser = await _userDataAccess.FindByUsername(entity.Username);
            if (existedUser == null) throw new BitqapBusinessException("User not found", "USER_NOT_FOUND");
            var result = await _userDataAccess.Update(entity);
            return result;
        }

        public async Task<User> GetUserById(long id)
        {
            _logger.Log(NLog.LogLevel.Debug, "Service called for GetUserById", default(Exception));
            return await _userDataAccess.FindById(id);
        }
    }
}
