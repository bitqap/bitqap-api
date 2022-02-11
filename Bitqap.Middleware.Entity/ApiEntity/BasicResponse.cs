using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitqap.Middleware.Entity.ApiEntity
{
    public class BasicResponse
    {
        public string RequestId { get; set; }
        public string Command { get; set; }
        public string Message { get; set; }
    }
}
