using Bitqap.Middleware.Entity.ApiEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitqap.Middleware.Business.Services
{
    public interface IUserService
    {
        Task<User> RegisterNewUser(User entity);
        Task<User> LoginUser(LoginRequest entity);
    }
}
