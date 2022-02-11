using Bitqap.Middleware.Entity.ApiEntity;
using Bitqap.Middleware.Entity.BusinessEntity;

namespace Bitqap.Middleware.Business.Services
{
    public interface IAccountService
    {
        Task<Account> CreateAccount(Account entity);
        Task<Account> FindAccountByAccountKey(string accountKey);
        Task<Account> FindAccountByUserId(string userId);
        Task<Account> UpdateAccount(Account entity);
        Task DeleteAccountById(long id);
        Task<IEnumerable<Account>> GelAllAccounts();
        Task<Account> FindAccountById(long id);
        Task<BasicResponse> RequestAccountBalanceByAccountKey(string accountKey, long userId);
        Task<Object> GetRequestedBalance(string key, long userId);
        Task<BasicResponse> RequestTransactionsByAccountKey(string accountKey, long userId);
        Task<Object> GetRequestedTransactions(string key, long userId);
    }
}
