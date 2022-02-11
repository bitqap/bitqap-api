using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitqap.Middleware.Entity.SocketEntity
{
    public  class GetTransactionsRequest
    {
        [JsonProperty("command")]
        public string Commant { get; set; }
        [JsonProperty("account")]
        public string AccountKey { get; set; }
        [JsonProperty("messageType")]
        public string MessageType { get; set; }
        [JsonProperty("requestID")]
        public string RequestId { get; set; }
    }
}
