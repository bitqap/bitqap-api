using Bitqap.Middleware.Entity.ApiEntity;
using Bitqap.Middleware.Entity.BusinessEntity;

namespace Bitqap.Middleware.Business.Services
{
    public interface IUserService
    {
        Task<User> RegisterNewUser(User entity);
        Task<User> LoginUser(LoginRequest entity);
        Task<User> UpdateUser(User entity);
        Task<User> GetUserById(long id);
    }
}
