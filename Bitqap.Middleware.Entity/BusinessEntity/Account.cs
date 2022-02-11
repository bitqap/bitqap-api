using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitqap.Middleware.Entity.BusinessEntity
{
	public class Account
	{
		[Required]
		public long Id { get; set; }
		[Required]
		public long UserId { get; set; }
		[Required]
		public string AccountKey { get; set; }
		public string AccountName { get; set; }
		public DateTime CreateDate { get; set; }
	}
}
