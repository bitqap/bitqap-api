using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitqap.Middleware.Entity.BusinessEntity
{
	public class AccountBalance
	{
		public long Id { get; set; }
		public long UserId { get; set; }
		public string AccountKey { get; set; }
		public string AccountName { get; set; }
		public decimal Balance { get; set; }
	}
}
