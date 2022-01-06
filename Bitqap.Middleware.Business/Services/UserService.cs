using Bitqap.Middleware.Business.DataAccess;
using Bitqap.Middleware.Entity.ApiEntity;
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

            var result = await _userDataAccess.Create(entity);

            return result;
        }
    }
}
