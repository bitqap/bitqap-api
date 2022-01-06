using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitqap.Middleware.Business.DataAccess
{
    public interface IBaseDateAccess<T> where T : class
    {
        Task<T> Create(T entity);
        Task<T> Update(T entity);
        Task DeleteById(long id);
        Task<IQueryable<T>> GelAll();
        Task<T> FindById(long id);
    }
}
