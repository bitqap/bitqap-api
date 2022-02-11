using Bitqap.Middleware.Entity.BusinessEntity;

namespace Bitqap.Middleware.Business.DataAccess
{
    public interface IAccountDataAccess : IBaseDataAccess<Account>
    {
        Task<Account> FindByAccountKey(string accountKey);
        Task<Account> FindByUserId(string userId);
    }
}
