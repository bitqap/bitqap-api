namespace Bitqap.Middleware.Business.DataAccess
{
    public interface IBaseDataAccess<T> where T : class
    {
        Task<T> Create(T entity);
        Task<T> Update(T entity);
        Task DeleteById(long id);
        Task<IEnumerable<T>> GelAll();
        Task<T> FindById(long id);
    }
}
