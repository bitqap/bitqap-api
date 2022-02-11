using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitqap.Middleware.Entity.SocketEntity
{
	public class MsgPayload
	{
		public long Id { get; set; }
		public string MethodName { get; set; }
		public string RequestKey { get; set; }
		public string Payload { get; set; }
		public MsgDirection Direction { get; set; }
		public DateTime CreateDateTime { get; set; } = DateTime.Now;
		public long UserId { get; set; }
	}

	public enum MsgDirection
    {
		SENT,
		RECEIVED
    }
}
