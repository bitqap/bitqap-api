namespace Bitqap.Middleware.Entity.SocketEntity
{
    public class GetBalanceResponse
    {
        //[JsonPropertyName("command")]
        public string Command;
        public string ResponseID;

        //[JsonPropertyName("commandCode")]
        public string CommandCode;

        //[JsonPropertyName("messageType")]
        public string MessageType;

        //[JsonPropertyName("status")]
        public string Status;

        //[JsonPropertyName("destinationSocket")]
        public string DestinationSocket;

        //[JsonPropertyName("result")]
        public Result Result;
    }

    public class Result
    {
        //[JsonPropertyName("publicKeyHASH256")]
        public string PublicKeyHASH256;

        //[JsonPropertyName("balance")]
        public string Balance;
    }
}
