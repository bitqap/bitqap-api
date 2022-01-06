using Bitqap.Middleware.Business.DataAccess;
using Bitqap.Middleware.Entity.ApiEntity;
using Bitqap.Middleware.Business.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitqap.Middleware.Business.Services
{
    public class UserService : IUserService
    {
        readonly IUserDataAccess _userDataAccess;
        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public UserService(IUserDataAccess userDataAccess)
        {
            _userDataAccess = userDataAccess;
        }

        [Obsolete]
        public async Task<User> RegisterNewUser(User entity)
        {
            _logger.Log(NLog.LogLevel.Info, "Service called for create new User", default(Exception));
            var existedUser = await _userDataAccess.FindByUsername(entity.Username);
            if (existedUser != null) throw new BitqapBusinessException("User already exists", "USER_EXISTS");
            var result =  await _userDataAccess.Create(entity);

            return result;
        }

        public async Task<User> LoginUser(LoginRequest entity)
        {
            _logger.Log(NLog.LogLevel.Info, "Service called for login user", default(Exception));
            var existedUser = await _userDataAccess.FindByUsername(entity.UserName);
            if (existedUser == null) throw new BitqapBusinessException("Invalid username or password", "INVALID_LOGIN");
            if (!string.Equals(existedUser.Password, entity.Password, StringComparison.InvariantCulture)) throw new BitqapBusinessException("Invalid username or password", "INVALID_LOGIN");
            
            return existedUser;
        }
    }
}
