namespace Bitqap.Middleware.Business.Utils
{
    public class BitqapBusinessException : System.Exception
    {
        public String ErrorCode = "";

        public BitqapBusinessException() : base()
        {
        }

        public BitqapBusinessException(string message, string code) : base(message)
        {
            this.ErrorCode = code;
        }
        public BitqapBusinessException(string message, Exception inner, string code) : base(message, inner)
        {
            this.ErrorCode = code;
        }
    }
}
