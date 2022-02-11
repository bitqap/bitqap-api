using Newtonsoft.Json;

namespace Bitqap.Middleware.Entity.SocketEntity
{
    public  class GetBalanceRequest
    {
        [JsonProperty("command")]
        public string Commant { get; set; }
        [JsonProperty("ACCTNUM")]
        public string AccountKey { get; set; }
        [JsonProperty("messageType")]
        public string MessageType { get; set; }
        [JsonProperty("requestID")]
        public string RequestId { get; set; }
    }
}
