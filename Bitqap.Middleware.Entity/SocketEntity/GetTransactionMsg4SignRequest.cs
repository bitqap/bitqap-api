using Newtonsoft.Json;

namespace Bitqap.Middleware.Entity.SocketEntity
{
    public class TrResult
    {
        [JsonProperty("forReciverData")]
        public string ForReciverData;

        [JsonProperty("forSenderData")]
        public string ForSenderData;
    }

    public class GetTransactionMsg4SignRequest
    {
        [JsonProperty("command")]
        public string Command;

        [JsonProperty("status")]
        public int Status;

        [JsonProperty("destinationSocket")]
        public string DestinationSocket;

        [JsonProperty("result")]
        public TrResult Result;
    }


}
