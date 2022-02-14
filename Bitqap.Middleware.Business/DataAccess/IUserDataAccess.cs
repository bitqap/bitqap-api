using Bitqap.Middleware.Entity.BusinessEntity;

namespace Bitqap.Middleware.Business.DataAccess
{
    public interface IUserDataAccess : IBaseDataAccess<User>
    {
        Task<User> FindByUsername(string username);
    }
}
