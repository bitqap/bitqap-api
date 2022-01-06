using Bitqap.Middleware.Entity.ApiEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitqap.Middleware.Business.DataAccess
{
    public interface IUserDataAccess: IBaseDataAccess<User>
    {
        Task<User> FindByUsername(string username);
    }
}
